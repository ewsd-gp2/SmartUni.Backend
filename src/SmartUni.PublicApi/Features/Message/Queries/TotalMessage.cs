using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using static System.Net.Mime.MediaTypeNames;

namespace SmartUni.PublicApi.Features.Message.Queries;

public class TotalMessage
{
    public record Response(
        string SenderId,
        int MessageCount,
        string SenderType,
        string SenderName,
        string? Major,
        string? Image
    );

    public sealed class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/message", HandleAsync)
                .Produces<List<Response>>()
                .WithTags("Report");
        }

        private static async Task<Results<Ok<List<Response>>, NotFound>> HandleAsync(
            SmartUniDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var messageGroups = await dbContext.ChatMessage
                 .Where(m => m.Timestamp >= sevenDaysAgo)
                .GroupBy(m => new { m.SenderId, m.SenderType, m.SenderName })
                .ToListAsync(cancellationToken);

            var results = new List<Response>();

            foreach (var group in messageGroups)
            {
                string? Image = null;
                string major = string.Empty;

                if (group.Key.SenderType == Enums.SenderType.Student)
                {
                    var studentData = await dbContext.Student
    .Where(s => s.Id.ToString() == group.Key.SenderId && !s.IsDeleted)
    .Select(s => new { s.Image, s.Major })
    .FirstOrDefaultAsync(cancellationToken);

                    if (studentData != null)
                    {
                         Image = studentData.Image != null
                            ? Convert.ToBase64String(studentData.Image)
                            : null;

                         major = studentData.Major.ToString();
                    }

                }
                else if (group.Key.SenderType == Enums.SenderType.Tutor)
                {
                    var TutorData = await dbContext.Tutor
    .Where(s => s.Id.ToString() == group.Key.SenderId && !s.IsDeleted)
    .Select(s => new { s.Image, s.Major })
    .FirstOrDefaultAsync(cancellationToken);
                    if (TutorData != null)
                    {
                         Image = TutorData.Image != null
                            ? Convert.ToBase64String(TutorData.Image)
                            : null;

                         major = TutorData.Major.ToString();
                    }

                }
                results.Add(new Response(
                    SenderId: group.Key.SenderId,
                    MessageCount: group.Count(),
                    SenderType: group.Key.SenderType.ToString(),
                    SenderName: group.Key.SenderName,
                    Major: major,
                    Image: Image
                    
                ));
            }
            if (!results.Any())
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(results);
        }
    }
}
