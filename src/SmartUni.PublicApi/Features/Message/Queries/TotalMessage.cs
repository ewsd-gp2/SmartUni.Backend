using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Linq;

namespace SmartUni.PublicApi.Features.Message.Queries
{
    public class TotalMessage
    {
        private record Response(string SenderId,int MessageCount,string SenderType,string senderName);
        //private record SenderType(string SenderId,Enums.SenderType senderType);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/message", HandleAsync)
                    .Produces<Results<IResult, NotFound>>()
                    .WithTags(nameof(Report));
            }
            
            public static async Task<Results<IResult, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                   SmartUniDbContext dbContext,
                  CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to get all allocations");
                IEnumerable<Response> messageCounts = await dbContext.ChatMessage
    .GroupBy(m => new { m.SenderId, m.SenderType, m.SenderName })
    .Select(g => new Response(
        g.Key.SenderId,
        g.Count(),
        g.Key.SenderType.ToString(),
        g.Key.SenderName
    ))
    .ToListAsync();

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

