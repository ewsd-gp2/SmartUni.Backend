using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Tutor.Commands
{
    public class EditTutor
    {
        private sealed record Request(
            string Name,
            string Email,
            string PhoneNumber,
            Enums.GenderType Gender,
            Enums.MajorType Major);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/tutor/{id:guid}",
                        ([FromRoute] Guid id, [FromBody] Request request, [FromServices] ILogger<Endpoint> logger,
                                [FromServices] SmartUniDbContext dbContext, CancellationToken cancellationToken) =>
                            HandleAsync(id, request, logger, dbContext, cancellationToken))
                    .WithDescription("Update an existing tutor")
                    .Accepts<Request>("application/json")
                    .Produces(200)
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .Produces<NotFound>(404)
                    .WithTags(nameof(Tutor));
            }

            private static async Task<Results<Ok, IResult>> HandleAsync(
                Guid id,
                Request request,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to edit tutor with ID: {Id} and request: {Request}", id, request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }

                Tutor? tutor = await dbContext.Tutor.FindAsync([id], cancellationToken);

                if (tutor is null)
                {
                    logger.LogWarning("Tutor with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                tutor.Name = request.Name;
                tutor.Email = request.Email;
                tutor.PhoneNumber = request.PhoneNumber;
                tutor.Gender = request.Gender;
                tutor.Major = request.Major;
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
                RuleFor(x => x.Gender).IsInEnum();
                RuleFor(x => x.Major).IsInEnum();
            }
        }
    }
}