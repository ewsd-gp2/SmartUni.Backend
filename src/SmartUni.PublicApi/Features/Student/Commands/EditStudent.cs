using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Student.Commands
{
    public class EditStudent
    {
        private sealed record Request(string Name, string Email, string PhoneNumber, string Gender, string Major);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/student/{id:guid}",
                        ([FromRoute] Guid id, [FromBody] Request request, [FromServices] ILogger<Endpoint> logger,
                                [FromServices] SmartUniDbContext dbContext, ClaimsPrincipal claims, CancellationToken cancellationToken) =>
                            HandleAsync(id, request, logger, dbContext, claims, cancellationToken))
                    .WithDescription("Update an existing student")
                    .Accepts<Request>("application/json")
                    .Produces(200)
                    .RequireAuthorization("api")
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .Produces<NotFound>(404)
                    .WithTags(nameof(Student));
            }

            private static async Task<Results<Ok, IResult>> HandleAsync(
                Guid id,
                Request request,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                ClaimsPrincipal claims,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to edit student with ID: {Id} and request: {Request}", id, request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult);
                }

                Student? student = await dbContext.Student.Include(x => x.Identity).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (student is null)
                {
                    logger.LogWarning("Student with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                student.UpdateStudentName(request.Name);
                student.Identity.Email = request.Email;
                student.Identity.PhoneNumber = request.PhoneNumber;
                student.UpdateStudentMajor(request.Major);
                student.UpdateStudentGender(request.Gender);
                student.UpdatedOn= DateTime.UtcNow;
                student.UpdatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier));
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully edited student with ID: {Id}", id);

                return TypedResults.Ok();
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.PhoneNumber).NotEmpty();
                RuleFor(x => x.Gender).IsInEnum();
                RuleFor(x => x.Major).IsInEnum();
            }
        }
    }
}
