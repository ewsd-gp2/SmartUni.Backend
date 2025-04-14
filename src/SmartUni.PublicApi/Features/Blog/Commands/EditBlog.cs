using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Blog.Commands
{
    public class EditBlog
    {
        private class Request
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public IFormFile? CoverImage { get; set; }
            public IFormFile? Attachment { get; set; }
            public string Type { get; set; }
        }

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/blog/{blogId:guid}", HandleAsync)
                    .RequireAuthorization("api")
                    .Accepts<Request>("multipart/form-data")
                    .DisableAntiforgery()
                    .Produces<ValidationFailure>(400)
                    .Produces<Ok>(200)
                    .WithTags(nameof(Blog));
            }

            private static async Task<IResult> HandleAsync(
                ClaimsPrincipal claims,
                [FromRoute] Guid blogId,
                [FromForm] Request request,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken
            )
            {
                logger.LogInformation("Submitted to edit blog with title {Title}", request.Title);
                var blog = await dbContext.Blog.FindAsync([blogId], cancellationToken);
                if (blog == null) return TypedResults.NotFound();
                var userId = Guid.Parse(claims.FindFirst("identityId")?.Value ??
                                        throw new InvalidOperationException());

                if (!claims.Identity.IsAuthenticated) return TypedResults.Unauthorized();
                if (blog.CreatedBy != userId) return TypedResults.Forbid();

                var validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Validation failed for creating blog with errors: {@Errors}",
                        validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }

                BaseUser? author = await dbContext.Users.Include(x => x.Staff)
                    .Include(x => x.Tutor)
                    .Include(x => x.Student)
                    .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

                blog.Title = request.Title;
                blog.Content = request.Content;
                blog.CoverImage = request.CoverImage != null ? GetFileArray(request.CoverImage) : null;
                blog.Attachment = request.Attachment != null ? GetFileArray(request.Attachment) : null;
                blog.AttachmentName = request.Attachment?.FileName;
                blog.UpdatedBy = userId;
                blog.UpdatedOn = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);

                return TypedResults.Ok();
            }
        }

        private class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(m => m.Title).NotEmpty().WithMessage("Title is required");
                RuleFor(m => m.Content).NotEmpty().WithMessage("Content is required");
                RuleFor(m => m.Type).NotEmpty().IsEnumName(typeof(Enums.BlogType)).WithMessage(
                    "Type is required and must be one of the following values : ['KnowledgeSharing', 'Announcement', 'NewsLetter']");
            }
        }

        private static byte[] GetFileArray(IFormFile file)
        {
            using MemoryStream ms = new();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
}