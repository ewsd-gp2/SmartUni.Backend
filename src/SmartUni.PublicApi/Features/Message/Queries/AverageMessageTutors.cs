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
        double MessageCount,
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

            var tutorMessages = await dbContext.ChatMessage
    .Where(m => m.SenderType == Enums.SenderType.Tutor)
    .GroupBy(m => new { m.SenderId, m.SenderName })
    .ToListAsync(cancellationToken);

            var results = new List<Response>();

            foreach (var group in tutorMessages)
            {
                // All messages for this tutor
                var allMessages = group.ToList();

                // Unique dates the tutor sent messages
                var totalDays = allMessages
                    .Select(m => m.Timestamp.Date)
                    .Distinct()
                    .Count();

                // Calculate average
                double averageMessageCount = totalDays > 0
                    ? (double)allMessages.Count / totalDays
                    : 0;

                var tutor = await dbContext.Tutor
                    .Where(t => t.Id.ToString() == group.Key.SenderId && !t.IsDeleted)
                    .Select(t => new { t.Image })
                    .FirstOrDefaultAsync(cancellationToken);

                string? base64Image = tutor?.Image != null
                    ? Convert.ToBase64String(tutor.Image)
                    : null;

                results.Add(new Response(
                    SenderId: group.Key.SenderId,
                    MessageCount: Math.Round(averageMessageCount, 2),
                    SenderName: group.Key.SenderName,
                    Image: base64Image
                ));
            }


            return TypedResults.Ok(results);

        }
    }
}
