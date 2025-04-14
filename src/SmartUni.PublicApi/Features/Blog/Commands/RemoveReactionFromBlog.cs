using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Blog.Commands
{
    public class RemoveReactionFromBlog
    {
        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapDelete("/blog/{blogId:guid}/like/", HandleAsync)
                    .WithDescription("Remove reaction from blog")
                    .Produces(401)
                    .Produces(404)
                    .WithTags(nameof(Blog));
            }

            private static async Task<IResult> HandleAsync(
                [FromRoute] Guid blogId,
                ClaimsPrincipal? claims,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken = default!)
            {
                logger.LogInformation("Adding like to blog with ID: {BlogId}", blogId);

                if (claims?.Identity?.IsAuthenticated != true)
                {
                    logger.LogWarning("Guest cannot react to blog");
                    return TypedResults.Unauthorized();
                }

                var userId = Guid.Parse(claims.FindFirst("identityId")?.Value);
                var blog = await dbContext.Blog
                    .Include(x => x.Reactions)
                    .FirstOrDefaultAsync(x => x.Id == blogId, cancellationToken);

                if (blog is null)
                {
                    logger.LogWarning("Blog with ID {BlogId} not found", blogId);
                    return TypedResults.NotFound();
                }

                if (blog.Reactions.All(r => r.ReacterId != userId))
                {
                    logger.LogInformation("User {UserId} has not reacted to Blog {BlogId}", userId, blogId);
                    return TypedResults.BadRequest();
                }

                blog.Reactions.RemoveAll(x => x.ReacterId == userId);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("User {UserId} removed reaction from blog {BlogId}", userId, blogId);

                return TypedResults.Ok();
            }
        }
    }
}