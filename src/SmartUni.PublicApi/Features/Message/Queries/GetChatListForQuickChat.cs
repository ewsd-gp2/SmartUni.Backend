using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Message.Queries
{
    public class GetChatListForQuickChat
    {
        public record Response(
    string ChatRoomId,
    string LastMessage,
    DateTime Timestamp,
    string SenderId,
    string OtherUserId,
    string OtherUserName,
    byte[]? OtherUserImage
     );
        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/message/quickchatlist/{userId}", HandleAsync)
                    .RequireAuthorization("api")
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
                    if (!Guid.TryParse(userId, out var userGuid))
                    {
                        return TypedResults.NotFound(); 
                    }

                    var students = await dbContext.Student
                        .Where(s => s.Id != userGuid) 
                        .OrderBy(s => Guid.NewGuid())  
                        .Take(5)
                        .ToListAsync(cancellationToken);

                    var randomUsers = students.Select(s => new Response(
                        ChatRoomId: "Default",
                        LastMessage: "",
                        Timestamp: DateTime.Now,
                        SenderId: userId,
                        OtherUserId: s.Id.ToString(),
                        OtherUserName: s.Name,
                        OtherUserImage: s.Image
                    )).ToList();

                    return TypedResults.Ok<IEnumerable<Response>>(randomUsers);
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

                    var parts = message.ChatRoomId.Split('_');
                    var otherUserId = parts.FirstOrDefault(p => p != userId && p != "chat");

                    if (otherUserId == null) continue;

                    // Try to find name and image
                    var info = await dbContext.Student
                        .Where(s => s.Id.ToString() == otherUserId)
                        .Select(s => new { s.Image, s.Name })
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? await dbContext.Tutor
                        .Where(t => t.Id.ToString() == otherUserId)
                        .Select(t => new { t.Image, t.Name })
                        .FirstOrDefaultAsync(cancellationToken);

                    responseList.Add(new Response(
                        message.ChatRoomId,
                        message.Content,
                        message.Timestamp,
                        message.SenderId,
                        otherUserId,
                        info?.Name ?? "Unknown",
                        info?.Image
                    ));
                }

                return TypedResults.Ok<IEnumerable<Response>>(responseList);
            }
        }

    }
}
