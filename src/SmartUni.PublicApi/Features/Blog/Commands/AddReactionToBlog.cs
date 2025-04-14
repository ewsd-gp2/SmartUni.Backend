using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Blog.Commands
{
    public class AddReactionToBlog
    {
        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/blog/{blogId:guid}/like/", HandleAsync)
                    .WithDescription("Add reaction to blog")
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

                if (blog.Reactions.Any(r => r.ReacterId == userId))
                {
                    logger.LogInformation("User {UserId} has already reacted to Blog {BlogId}", userId, blogId);
                    return TypedResults.BadRequest();
                }

                var reaction = new BlogReaction { Id = Guid.NewGuid(), BlogId = blogId, ReacterId = userId };

                blog.Reactions.Add(reaction);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    blog.CreatedBy == userId
                        ? "User {UserId} reacted to their own blog"
                        : "User {UserId} added the reaction to Blog {BlogId}",
                    userId, blog.Id);

                return TypedResults.Ok();
            }
        }
    }
}