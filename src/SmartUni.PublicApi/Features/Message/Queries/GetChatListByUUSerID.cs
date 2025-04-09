using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Message.Queries
{
    public class UserChatList
    {
        public record Response(string ChatRoomId, string LastMessage, string Sender, DateTime Timestamp);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/message/chatlist/{userId}", HandleAsync)
                    .Produces<Results<Ok<IEnumerable<Response>>, NotFound>>()
                    .WithTags(nameof(Message));
            }

            public static async Task<Results<Ok<IEnumerable<Response>>, NotFound>> HandleAsync(
                string userId,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching chat list for user {UserId}", userId);

                // Get chat rooms the user participates in
                var chatRoomIds = await dbContext.ChatParticipant
                    .Where(p => p.UserId == userId)
                    .Select(p => p.ChatRoomId)
                    .ToListAsync(cancellationToken);

                if (!chatRoomIds.Any())
                {
                    logger.LogWarning("No chat rooms found for user {UserId}", userId);
                    return TypedResults.NotFound();
                }

                // Get latest message per chat room
                var latestMessages = await dbContext.ChatMessage
                    .Where(m => chatRoomIds.Contains(m.ChatRoomId))
                    .GroupBy(m => m.ChatRoomId)
                    .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                    .ToListAsync(cancellationToken);

                var response = latestMessages.Select(m => new Response(
                    m.ChatRoomId,
                    m.Content,
                    m.SenderId,
                    m.Timestamp
                ));

                return TypedResults.Ok(response);
            }
        }
    }
}
