using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Message.Queries
{
    public class TotalMessage
    {
        private record Response(string SenderId,int MessageCount);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/message", HandleAsync)
                    .Produces<Results<IResult, NotFound>>()
                    .WithTags(nameof(Message));
            }
            
            public static async Task<Results<IResult, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                   SmartUniDbContext dbContext,
                  CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to get all allocations");
                IEnumerable<Response> messageCounts = await dbContext.ChatMessage
    .GroupBy(m => m.SenderId) 
    .Select(g => new Response(g.Key, g.Count()))
    .ToListAsync(cancellationToken);

                if (!messageCounts.Any())
                {
                    logger.LogWarning("No messages found");
                    return TypedResults.NotFound();
                }

                logger.LogInformation("Successfully retrieved message counts. Found {MessageCount} senders", messageCounts.Count());
                return TypedResults.Ok(messageCounts);
            }

        }

    }
}

