using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Linq;

namespace SmartUni.PublicApi.Features.Message.Queries;

public class AverageMessageTutors
{
    public record Response(
        string SenderId,
        int MessageCount,
        string SenderName,
        string? Image
    );

    public sealed class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/message/averageMessage", HandleAsync)
                .RequireAuthorization("api")
                .Produces<List<Response>>()
                .WithTags("Report");
        }

        private static async Task<Results<Ok<List<Response>>, NotFound>> HandleAsync(
            SmartUniDbContext dbContext,
            CancellationToken cancellationToken)
        {
            // Get all tutor messages
            var tutorMessages = await dbContext.ChatMessage
                .Where(m => m.SenderType == Enums.SenderType.Tutor)
                .ToListAsync(cancellationToken);

            // Get distinct sender IDs
            var tutorIds = tutorMessages.Select(m => m.SenderId).Distinct().ToList();

var tutors = await dbContext.Tutor
    .Where(u => tutorIds.Contains(u.Id.ToString()))
    .ToDictionaryAsync(u => u.Id, cancellationToken);

            // Calculate average messages per day
            var response = tutorMessages
                .GroupBy(m => new { m.SenderId, m.SenderName })
                .Select(g =>
                {
                    var totalMessages = g.Count();
                    var firstDate = g.Min(m => m.Timestamp).Date;
                    var lastDate = g.Max(m => m.Timestamp).Date;
                    var totalDays = (lastDate - firstDate).TotalDays + 1;

                    var averagePerDay = totalDays > 0
                        ? totalMessages / totalDays
                        : totalMessages;

                    tutors.TryGetValue(Guid.Parse(g.Key.SenderId), out var tutor);

                    return new Response(
                        SenderId: g.Key.SenderId,
                        MessageCount: (int)Math.Round(averagePerDay),
                        SenderName: g.Key.SenderName,
                        Image: Convert.ToBase64String(tutor?.Image)
                    );
                })
                .ToList();

            if (!response.Any())
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(response);
        }
    }
}
