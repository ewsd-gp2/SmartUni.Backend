using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Message.Queries
{
    public class UserChatList
    {
        public record Response(
    string ChatRoomId,
    string LastMessage,
    DateTime Timestamp,
    string SenderId,
    List<byte[]> ChatProfileImages
);
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

                var chatRoomIds = await dbContext.ChatParticipant
                    .Where(p => p.UserId == userId)
                    .Select(p => p.ChatRoomId)
                    .ToListAsync(cancellationToken);

                if (!chatRoomIds.Any())
                {
                    return TypedResults.NotFound();
                }

                var latestMessages = await dbContext.ChatMessage
                    .Where(m => chatRoomIds.Contains(m.ChatRoomId))
                    .GroupBy(m => m.ChatRoomId)
                    .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                    .ToListAsync(cancellationToken);

                var responseList = new List<Response>();

                foreach (var message in latestMessages)
                {
                    if (message == null) continue;

                    // Get the other participant in this chat room
                    var otherUserIds = await dbContext.ChatParticipant
     .Where(p => p.ChatRoomId == message.ChatRoomId && p.UserId != userId)
     .Select(p => p.UserId)
     .ToListAsync(cancellationToken);

                    var profileImages = new List<byte[]>();

                    foreach (var otherUserId in otherUserIds)
                    {
                        byte[]? image = await dbContext.Student
                            .Where(s => s.Id.ToString() == otherUserId)
                            .Select(s => s.Image)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (image == null)
                        {
                            image = await dbContext.Tutor
                                .Where(t => t.Id.ToString() == otherUserId)
                                .Select(t => t.Image)
                                .FirstOrDefaultAsync(cancellationToken);
                        }

                        if (image != null)
                        {
                            profileImages.Add(image);
                        }
                    }

                    responseList.Add(new Response(
                        message.ChatRoomId,
                        message.Content,
                        message.Timestamp,
                        message.SenderId,
                        profileImages
                    ));
                }

                return TypedResults.Ok<IEnumerable<Response>>(responseList);
            }
        }
    }
}
