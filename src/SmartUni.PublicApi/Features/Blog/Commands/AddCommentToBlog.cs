using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Blog.Commands
{
    public class AddCommentToBlog
    {
        private sealed record Request(string Comment);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/blog/{blogId:guid}/comment/", HandleAsync)
                    .WithDescription("Add comment to blog")
                    .Produces(401)
                    .Produces(404)
                    .WithTags(nameof(Blog));
            }

            private static async Task<IResult> HandleAsync(
                [FromRoute] Guid blogId,
                [FromBody] Request request,
                ClaimsPrincipal? claims,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken = default!)
            {
                logger.LogInformation("Adding comment to blog with ID: {BlogId}", blogId);

                if (claims?.Identity?.IsAuthenticated != true)
                {
                    logger.LogWarning("Guest cannot comment to blog");
                    return TypedResults.Unauthorized();
                }

                var userId = Guid.Parse(claims.FindFirst("identityId")?.Value);
                var blog = await dbContext.Blog
                    .Include(x => x.Comments)
                    .FirstOrDefaultAsync(x => x.Id == blogId, cancellationToken);

                if (blog is null)
                {
                    logger.LogWarning("Blog with ID {BlogId} not found", blogId);
                    return TypedResults.NotFound();
                }

                var comment = new BlogComment()
                {
                    Id = Guid.NewGuid(),
                    BlogId = blogId,
                    CommenterId = userId,
                    Comment = request.Comment,
                    CommentedOn = DateTime.UtcNow
                };

                blog.Comments.Add(comment);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    blog.CreatedBy == userId
                        ? "User {UserId} commented to their own blog"
                        : "User {UserId} commented to Blog {BlogId}",
                    userId, blog.Id);

                return TypedResults.Ok();
            }
        }
    }
}