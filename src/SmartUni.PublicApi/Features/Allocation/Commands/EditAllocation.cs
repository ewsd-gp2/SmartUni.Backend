using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Allocation.Commands
{
    public class EditAllocation
    {
        private sealed record Request(Guid Student_ID, Guid Tutor_ID, Guid Updated_By);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/allocation/{id:guid}",
                        ([FromRoute] Guid id, [FromBody] Request request, [FromServices] ILogger<Endpoint> logger,
                                [FromServices] SmartUniDbContext dbContext, CancellationToken cancellationToken) =>
                            HandleAsync(id, request, logger, dbContext, cancellationToken))
                    .Produces<Ok>()
                    .Produces<BadRequest<ValidationResult>>(StatusCodes.Status400BadRequest)
                    .Produces<NotFound>(StatusCodes.Status404NotFound)
                    .ProducesValidationProblem()
                    .WithTags(nameof(Allocation));
            }

            private static async Task<Results<Ok, IResult>> HandleAsync(
                Guid id,
                Request request,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to edit allocation with ID: {Id} and request: {Request}", id, request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult);
                }

                Allocation? allocation = await dbContext.Allocations.FindAsync([id], cancellationToken);

                if (allocation is null)
                {
                    logger.LogWarning("Allocation with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                allocation.UpdateAllocation(request.Student_ID,request.Tutor_ID,request.Updated_By,DateTime.UtcNow);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully edited allocation with ID: {Id}", id);

                return TypedResults.Ok();
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Student_ID).NotEmpty();
                RuleFor(x => x.Tutor_ID).NotEmpty();
                RuleFor(x => x.Updated_By).NotEmpty();
            }
        }
    }
}
