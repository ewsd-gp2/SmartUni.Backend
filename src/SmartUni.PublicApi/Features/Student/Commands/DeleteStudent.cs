using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Student.Commands
{
    public class DeleteStudent
    {
        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapDelete("/student/{id:guid}", HandleAsync)
                    .Produces<NotFound>()
                    .WithTags(nameof(Student));
            }

            private static async Task<Results<NoContent, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                Guid id,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to delete student with ID: {Id}", id);

                Student? student = await dbContext.Student.FindAsync([id], cancellationToken);
                if (student is null)
                {
                    logger.LogWarning("Student with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                student.DeleteStudent();
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully deleted student with ID: {Id}", id);
                return TypedResults.NoContent();
            }
        }
    }
}
