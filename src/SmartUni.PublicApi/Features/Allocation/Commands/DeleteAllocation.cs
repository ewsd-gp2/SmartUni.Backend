using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Student;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;
using Student = SmartUni.PublicApi.Features.Student.Student;


namespace SmartUni.PublicApi.Features.Allocation.Commands
{
    public class DeleteAllocation
    {
        //private sealed record Request(Guid? AllocationID);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/allocation/{id:guid}",
                        ([FromRoute] Guid id, [FromServices] ILogger<Endpoint> logger,
                                [FromServices] SmartUniDbContext dbContext, ClaimsPrincipal claims, CancellationToken cancellationToken) =>
                            HandleAsync(id, logger, dbContext, claims, cancellationToken))
                    .WithDescription("Delete an existing student")
                    .Produces(200)
                    .RequireAuthorization("api")
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .Produces<NotFound>(404)
                    .WithTags(nameof(Allocation));
            }

            private static async Task<Results<Ok, IResult>> HandleAsync(
                Guid id,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                ClaimsPrincipal claims,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to delete allocation with ID: {Id} and request: {Request}", id);

                Allocation? allocation = await dbContext.Allocation.FindAsync([id], cancellationToken);

                if (allocation is null)
                {
                    logger.LogWarning("Allocation with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                allocation.IsDeleted = true;
                allocation.UpdatedOn = DateTime.UtcNow;
                allocation.UpdatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier));
                //Student student = await dbContext.Student.FindAsync(allocation.StudentId, cancellationToken);
                //if (student is not null)
                //{
                //    student.isa

                //    student.UpdatedOn = DateTime.UtcNow;
                //    student.UpdatedBy = allocation.UpdatedBy; // reuse updater if applicable
                //}
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully deleted allocation with ID: {Id}", id);

                return TypedResults.Ok();
            }
        }

    }
}
