using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Tutor.Commands
{
    public sealed class CreateTutor
    {
        private sealed record Request(
            [FromForm(Name = "name")] string Name,
            [FromForm(Name = "email")] string Email,
            [FromForm(Name = "phoneNumber")] string PhoneNumber,
            [FromForm(Name = "gender")] string Gender,
            [FromForm(Name = "major")] string Major,
            [FromForm(Name = "password")] string Password,
            [FromForm(Name = "image")] IFormFile Image);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/tutor", HandleAsync)
                    .RequireAuthorization("api")
                    .WithDescription("Create new tutor")
                    .Accepts<Request>("multipart/form-data")
                    .Produces(201)
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .WithTags(nameof(Tutor))
                    .DisableAntiforgery();
            }

            private static async Task<IResult> HandleAsync(
                ClaimsPrincipal claims,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                [FromForm] Request request,
                UserManager<BaseUser> userManager,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to create a new tutor with email: {Email}", request.Email);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {@Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }

                Tutor tutor = MapToDomain(request);
                tutor.CreatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                             throw new InvalidOperationException(ClaimTypes.NameIdentifier));
                BaseUser user = new()
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    IsFirstLogin = true,
                    Tutor = tutor
                };

                IdentityResult result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    logger.LogInformation("Failed to create a new user with errors: {@Errors}", result.Errors);
                    return TypedResults.BadRequest(result.Errors);
                }

                logger.LogInformation("Successfully created a new tutor with ID: {Id}", tutor.Id);
                return TypedResults.Created($"/tutor/{tutor.Id}", tutor.Id);
            }

            private static Tutor MapToDomain(Request request)
            {
                Tutor tutor = new()
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Gender = Enum.Parse<Enums.GenderType>(request.Gender),
                    Major = Enum.Parse<Enums.MajorType>(request.Major),
                    Image = GetFileArray(request.Image)
                };
                return tutor;
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Tutor name is required")
                    .MaximumLength(50).WithMessage("Tutor name must not exceed 50 characters");

                RuleFor(x => x.Email).CustomEmailAddress();
                RuleFor(x => x.PhoneNumber).PhoneNumber();
                RuleFor(x => x.Gender).IsEnumName(typeof(Enums.GenderType));
                RuleFor(x => x.Major).IsEnumName(typeof(Enums.MajorType));
            }
        }

        private static byte[] GetFileArray(IFormFile file)
        {
            using MemoryStream ms = new ();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
}