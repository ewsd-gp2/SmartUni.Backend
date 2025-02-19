using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Tutor.Commands
{
    public class EditTutor
    {
        private sealed record Request(string Name, string Email, string PhoneNumber);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/tutor/{id:guid}",
                        ([FromRoute] Guid id, [FromBody] Request request, [FromServices] ILogger<Endpoint> logger,
                                [FromServices] SmartUniDbContext dbContext, CancellationToken cancellationToken) =>
                            HandleAsync(id, request, logger, dbContext, cancellationToken))
                    .Produces<Ok>()
                    .Produces<BadRequest<ValidationResult>>(StatusCodes.Status400BadRequest)
                    .Produces<NotFound>(StatusCodes.Status404NotFound)
                    .ProducesValidationProblem()
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
                    return TypedResults.BadRequest(validationResult);
                }

                Tutor? tutor = await dbContext.Tutor.FindAsync([id], cancellationToken);

                if (tutor is null)
                {
                    logger.LogWarning("Tutor with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                tutor.UpdateTutorName(request.Name);
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
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.PhoneNumber).NotEmpty().PhoneNumber();
            }
        }
    }
}