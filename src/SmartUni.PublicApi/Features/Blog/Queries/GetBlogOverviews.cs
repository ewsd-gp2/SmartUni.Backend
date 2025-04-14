using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;
using System.Text;

namespace SmartUni.PublicApi.Features.Blog.Queries
{
    public class GetBlogOverviews
    {
        private sealed record Response(
            Guid Id,
            string Title,
            string? CoverImage,
            string Type,
            DateTime CreatedOn,
            string AuthorName,
            string? AuthorAvatar);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/blog", HandleAsync)
                    .WithDescription("Get all blog overviews")
                    .Produces<List<Response>>(200)
                    .WithTags(nameof(Blog));
            }

            private static async Task<IResult> HandleAsync(
                ClaimsPrincipal? claims,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken = default!
            )
            {
                logger.LogInformation("Fetching all blog overviews");
                List<Response> responses;

                if (claims?.Identity is { IsAuthenticated: false })
                {
                    responses = await dbContext.Blog.Where(x => x.Type == Enums.BlogType.NewsLetter)
                        .Select(x => new Response(
                            x.Id, 
                            x.Title, 
                            Convert.ToBase64String(x.CoverImage ?? Array.Empty<byte>()),
                            x.Type.ToString(), 
                            x.CreatedOn, 
                            x.AuthorName,
                            Convert.ToBase64String(x.AuthorAvatar ?? Array.Empty<byte>())))
                        .ToListAsync(cancellationToken);

                    return TypedResults.Ok(responses);
                }

                responses = await dbContext.Blog
                    .Select(x => new Response(x.Id, x.Title, Convert.ToBase64String(x.CoverImage ?? Array.Empty<byte>()),
                        x.Type.ToString(), x.CreatedOn, x.AuthorName,
                        Convert.ToBase64String(x.AuthorAvatar ?? Array.Empty<byte>())))
                    .ToListAsync(cancellationToken);

                return TypedResults.Ok(responses);
            }
        }
    }
}