using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Blog.Queries
{
    public class GetBlogContent
    {
        private sealed record Response(
            Guid Id,
            string Title,
            string Content,
            string? CoverImage,
            string? AttachmentName,
            string Type,
            DateTime CreatedOn,
            string AuthorName,
            string? AuthorAvatar,
            List<BlogCommentResponse> Comments,
            int ReactionCount,
            bool IsLiked,
            bool CanEdit);

        private record BlogCommentResponse(
            string CommenterName,
            string CommenterAvatar,
            string Comment,
            DateTime CommentedOn);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/blog/{blogId:guid}", HandleAsync)
                    .WithDescription("Get blog content by id")
                    .Produces<Response>()
                    .Produces(404)
                    .Produces(401)
                    .WithTags(nameof(Blog));
            }

            private static async Task<IResult> HandleAsync(
                ClaimsPrincipal? claims,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                [FromRoute] Guid blogId,
                CancellationToken cancellationToken = default!)
            {
                logger.LogInformation("Fetching blog content with id {Id}", blogId);

                var blog = await dbContext.Blog
                    .Include(x => x.Comments).Include(blog => blog.Reactions)
                    .FirstOrDefaultAsync(x => x.Id == blogId, cancellationToken);

                if (blog?.Type != Enums.BlogType.NewsLetter && claims?.Identity is { IsAuthenticated: false })
                {
                    return TypedResults.Forbid();
                }


                Guid userId = Guid.Parse(claims?.FindFirstValue("identityId") ?? string.Empty);

                if (blog is null)
                {
                    return TypedResults.NotFound();
                }

                var response = new Response(
                    blog.Id,
                    blog.Title,
                    blog.Content,
                    Convert.ToBase64String(blog.CoverImage ?? []),
                    blog.AttachmentName,
                    blog.Type.ToString(),
                    blog.CreatedOn,
                    blog.AuthorName,
                    Convert.ToBase64String(blog.AuthorAvatar ?? []),
                    blog.Comments.Select(c =>
                            new BlogCommentResponse(UserHelper.GetUserNameByUserId(c.CommenterId, dbContext).Result,
                                Convert.ToBase64String(
                                    UserHelper.GetUserAvatarByUserId(c.CommenterId, dbContext).Result ?? []), c.Comment,
                                c.CommentedOn))
                        .ToList(),
                    blog.Reactions.Count,
                    blog.Reactions.Any(x => x.BlogId == blogId && x.ReacterId == userId),
                    claims?.Identity?.IsAuthenticated == true && blog.CreatedBy == userId
                );

                return TypedResults.Ok(response);
            }
        }
    }
}
