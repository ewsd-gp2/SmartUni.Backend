using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Net.Mail;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Blog.Commands
{
    public class CreateBlog
    {
        private sealed class Request
        {
            public required string Title { get; init; }
            public required string Content { get; init; }
            public IFormFile? CoverImage { get; init; }
            public IFormFile? Attachment { get; init; }
            public required string Type { get; init; }
        }

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/blog", HandleAsync)
                    .RequireAuthorization("api")
                    .Accepts<Request>("multipart/form-data")
                    .DisableAntiforgery()
                    .Produces<ValidationFailure>(400)
                    .Produces<Ok>(200)
                    .WithTags(nameof(Blog));
            }

            private static async Task<IResult> HandleAsync(
                ClaimsPrincipal claims,
                [FromForm] Request request,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken
            )
            {
                logger.LogInformation("Submitted to create new blog with title {Title}", request.Title);
                var userId = Guid.Parse(claims.FindFirst("identityId")?.Value ??
                                        throw new InvalidOperationException());
                
                var validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Validation failed for creating blog with errors: {@Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }
                
                Blog blog = MapToDomain(request, userId);
                BaseUser? author = await dbContext.Users.Include(x => x.Staff)
                    .Include(x => x.Tutor)
                    .Include(x => x.Student)
                    .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
                blog.AuthorName = author?.Role switch
                {
                    Enums.RoleType.Staff => author?.Staff.Name,
                    Enums.RoleType.Tutor => author?.Tutor.Name,
                    Enums.RoleType.Student => author?.Student.Name,
                } ?? throw new InvalidOperationException();
                
                blog.AuthorAvatar = author?.Role switch
                {
                    Enums.RoleType.Staff => author.Staff.Image,
                    Enums.RoleType.Tutor => author.Tutor.Image,
                    Enums.RoleType.Student => author.Student.Image
                } ?? [];
                
                await dbContext.Blog.AddAsync(blog, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                return TypedResults.Ok();
            }

            private static Blog MapToDomain(Request request, Guid userId)
            {
                return new Blog
                {
                    Title = request.Title,
                    Content = request.Content,
                    Type = (Enums.BlogType)Enum.Parse(typeof(Enums.BlogType), request.Type),
                    CoverImage = request.CoverImage != null ? GetFileArray(request.CoverImage) : null,
                    Attachment = request.Attachment != null ? GetFileArray(request.Attachment) : null,
                    AttachmentName = request.Attachment?.FileName,
                    CreatedBy = userId
                };
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