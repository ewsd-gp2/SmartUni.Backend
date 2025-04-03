using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Tutor.Queries
{
    public class GetTutorInfoById
    {
        private sealed record Response(
            Guid Id,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            string Major,
            string UserCode,
            string Image);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/tutor/{id:guid}", HandleAsync)
                    .RequireAuthorization("api")
                    .Produces<Ok<Response>>()
                    .Produces<NotFound>()
                    .WithTags(nameof(Tutor));
            }

            private static async Task<Results<Ok<Response>, NotFound>> HandleAsync(
                [FromRoute] Guid id,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching details for tutor with ID: {Id}", id);

                Tutor? tutor = await dbContext.Tutor.Include(x => x.Identity)
                    .Where(x => !x.IsDeleted).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (tutor is null)
                {
                    logger.LogWarning("Tutor with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                Response response = new(tutor.Id, tutor.Name, tutor.Identity.Email!,
                    tutor.Identity.PhoneNumber!,
                    tutor.Gender.ToString(),
                    tutor.Major.ToString(), tutor.UserCode, tutor.Image is null ? string.Empty : Convert.ToBase64String(tutor.Image));
                logger.LogInformation("Successfully fetched details for tutor with ID: {Id} with response: {Response}",
                    id, response);
                return TypedResults.Ok(response);
            }
        }
    }
}