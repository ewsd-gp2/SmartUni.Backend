using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Tutor.Commands
{
    public class EditTutor
    {
        private sealed record Request(
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            string Major);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/tutor/{id:guid}", HandleAsync)
                    .WithDescription("Update an existing tutor")
                    .Accepts<Request>("application/json")
                    .Produces(200)
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .Produces<NotFound>(404)
                    .WithTags(nameof(Tutor));
            }

            private static async Task<Results<Ok, IResult>> HandleAsync(
                [FromRoute] Guid id,
                [FromBody] Request request,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                ClaimsPrincipal claims,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to edit tutor with ID: {Id} and request: {Request}", id, request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }

                Tutor? tutor = await dbContext.Tutor.Where(x => !x.IsDeleted).Include(x => x.Identity)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (tutor is null)
                {
                    logger.LogWarning("Tutor with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                tutor.Name = request.Name;
                tutor.Identity.Email = request.Email;
                tutor.Identity.PhoneNumber = request.PhoneNumber;
                tutor.Gender = Enum.Parse<Enums.GenderType>(request.Gender);
                tutor.Major = Enum.Parse<Enums.MajorType>(request.Major);
                tutor.CreatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier));
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully edited tutor with ID: {Id}", id);

                return TypedResults.Ok();
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Email).CustomEmailAddress();
                RuleFor(x => x.PhoneNumber).PhoneNumber();
                RuleFor(x => x.Gender).IsEnumName(typeof(Enums.GenderType));
                RuleFor(x => x.Major).IsEnumName(typeof(Enums.MajorType));
            }
        }
    }
}