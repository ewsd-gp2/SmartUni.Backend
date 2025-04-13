using MediatR;
using Serilog;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Report.Queries
{
    public sealed class MostViewPag
    {
        private sealed record Response(string PageName, int ViewCount);
        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/pageview/top", HandleAsync)
                    .RequireAuthorization("api")
                    .WithDescription("Get most viewed pages")
                    .Produces<List<Response>>(200)
                    .WithTags("PageView")
                    .DisableAntiforgery();
            }

            private static async Task<IResult> HandleAsync(
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                var topPages = await dbContext.Page
                    .OrderByDescending(p => p.ViewCount)
                    .Select(p => new Response(p.PageName.ToString(), p.ViewCount))
                    .ToListAsync(cancellationToken);
                if (topPages != null)
                {
                    logger.LogInformation("There is no Data:", null);
                }
                logger.LogInformation("Get List of most view:", topPages);
                return TypedResults.Ok(topPages);
            }
        }

        
    }

}
