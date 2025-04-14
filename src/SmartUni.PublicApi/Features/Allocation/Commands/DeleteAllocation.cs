using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Allocation.Commands
{
    public class DeleteAllocation
    {
        private sealed record Request([FromBody] List<Guid> AllocationIds);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/allocation",
                        async ([FromBody] Request request,
                               [FromServices] ILogger<Endpoint> logger,
                               [FromServices] SmartUniDbContext dbContext,
                               ClaimsPrincipal claims,
                               CancellationToken cancellationToken) =>
                            await HandleAsync(request, logger, dbContext, claims, cancellationToken))
                    .WithDescription("Soft delete a list of allocations")
                    .Produces(200)
                    .RequireAuthorization("api")
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .WithTags(nameof(Allocation));
            }

            private static async Task<Results<Ok, BadRequest<List<ValidationFailure>>>> HandleAsync(
                Request request,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                ClaimsPrincipal claims,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Request to delete allocations: {@Ids}", request.AllocationIds);

                var validator = new Validator();
                var validationResult = await validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Validation failed: {@Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }

                var userId = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier));
                var allocations = await dbContext.Allocation
                    .Where(a => request.AllocationIds.Contains(a.Id))
                    .ToListAsync(cancellationToken);

                foreach (var allocation in allocations)
                {
                    allocation.IsDeleted = true;
                    allocation.UpdatedOn = DateTime.UtcNow;
                    allocation.UpdatedBy = userId;
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Successfully deleted {Count} allocations.", allocations.Count);

                return TypedResults.Ok();
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.AllocationIds)
                    .NotEmpty().WithMessage("At least one ID must be provided.")
                    .ForEach(idRule => idRule.NotEmpty().WithMessage("Invalid allocation ID."));
            }
        }
    }
}
