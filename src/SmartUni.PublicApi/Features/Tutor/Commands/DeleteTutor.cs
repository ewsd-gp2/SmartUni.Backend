using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Tutor.Commands
{
    public sealed class DeleteTutor
    {
        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapDelete("/tutor/{id:guid}", HandleAsync)
                    .Produces<NotFound>()
                    .WithTags(nameof(Tutor));
            }

            private static async Task<Results<Ok, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                Guid id,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to delete tutor with ID: {Id}", id);

                Tutor? tutor = await dbContext.Tutor.FindAsync([id], cancellationToken);
                if (tutor is null)
                {
                    logger.LogWarning("Tutor with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                tutor.DeleteTutor();
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully deleted tutor with ID: {Id}", id);
                return TypedResults.Ok();
            }
        }
    }
}