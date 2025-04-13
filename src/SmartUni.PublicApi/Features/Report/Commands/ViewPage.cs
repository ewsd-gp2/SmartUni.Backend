using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Report.Commands
{
    public sealed class PageView
    {
        private sealed record Request(
            [FromForm(Name = "pageName")] string PageName);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/pageview", HandleAsync)
                    .RequireAuthorization("api")
                    .WithDescription("Track page view")
                    .Accepts<Request>("multipart/form-data")
                    .Produces(200)
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .WithTags("PageView")
                    .DisableAntiforgery();
            }

            private static async Task<IResult> HandleAsync(
                SmartUniDbContext dbContext,
                [FromForm] Request request,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Tracking page view for: {Page}", request.PageName);

                ValidationResult validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Page view request failed validation: {@Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }

                var pageNameEnum = Enum.Parse<Enums.MostViewPage>(request.PageName);

                var pageView = await dbContext.Page
                    .FirstOrDefaultAsync(p => p.PageName == pageNameEnum, cancellationToken);

                if (pageView is null)
                {
                    pageView = new Page
                    {
                        Id = Guid.NewGuid(),
                        PageName = pageNameEnum,
                        ViewCount = 1
                    };

                    dbContext.Page.Add(pageView);
                }
                else
                {
                    pageView.ViewCount += 1;
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                return TypedResults.Ok(new { pageView.PageName, pageView.ViewCount });
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.PageName)
                    .NotEmpty()
                    .IsEnumName(typeof(Enums.MostViewPage), caseSensitive: false)
                    .WithMessage("Invalid page name");
            }
        }
    }

}
