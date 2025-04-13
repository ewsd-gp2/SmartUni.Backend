using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Extensions;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Report.Queries;

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
                .WithTags("Report")
                .DisableAntiforgery();
        }

        private static async Task<IResult> HandleAsync(
            SmartUniDbContext dbContext,
            ILogger<Endpoint> logger,
            CancellationToken cancellationToken)
        {
            var topPages = await dbContext.Page
                .OrderByDescending(p => p.ViewCount)
                .Select(p => new Response(p.PageName.GetDescription(), p.ViewCount))
                .ToListAsync(cancellationToken);

            if (topPages is null || !topPages.Any())
            {
                logger.LogInformation("No page view data found.");
                return TypedResults.Ok(new List<Response>());
            }

            logger.LogInformation("Returning list of most viewed pages: {@TopPages}", topPages);
            return TypedResults.Ok(topPages);
        }
    }
}
