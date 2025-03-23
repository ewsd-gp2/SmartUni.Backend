using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Meeting;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Workers
{
    public class MeetingWorker(IServiceScopeFactory scopeFactory, ILogger<MeetingWorker> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    SmartUniDbContext dbContext = scope.ServiceProvider.GetRequiredService<SmartUniDbContext>();
                    DateTime now = DateTime.UtcNow;

                    List<Meeting> expiredMeetings = await dbContext.Meeting
                        .Where(m => m.EndTime < now && m.Status == Enums.MeetingStatus.New)
                        .ToListAsync(stoppingToken);

                    if (expiredMeetings.Count != 0)
                    {
                        foreach (Meeting meeting in expiredMeetings)
                        {
                            meeting.Status = Enums.MeetingStatus.Completed;
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                        logger.LogInformation("Marked {MeetingCount} meetings as Completed.",
                            expiredMeetings.Count);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error updating expired meetings.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}