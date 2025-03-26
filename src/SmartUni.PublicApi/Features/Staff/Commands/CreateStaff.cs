using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SmartUni.PublicApi.Features.Staff.Commands
{
    public class CreateStaff
    {
        private sealed record Request(string Name, string Email, string PhoneNumber,string  Gender,string Password);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/staff", HandleAsync)
                    .RequireAuthorization("api")
                    .WithDescription("Create new staff")
                    .Accepts<Request>("application/json")
                    .Produces(201)
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .WithTags(nameof(Staff));
            }

            private static async Task<IResult> HandleAsync(
                ClaimsPrincipal claims,
                UserManager<BaseUser> userManager,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                Request request,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to create a new staff with request: {Request}", request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult);
                }

                Staff staff = MapToDomain(request);
                staff.CreatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                            throw new InvalidOperationException(ClaimTypes.NameIdentifier));
                BaseUser user = new()
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Staff = staff
                };
                IdentityResult result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    logger.LogInformation("Failed to create a new user with errors: {Errors}", result.Errors);
                    return TypedResults.BadRequest(result.Errors);
                }

                logger.LogInformation("Successfully created a new tutor with ID: {Id}", staff.Id);
                return TypedResults.Created();
                
            }

            private static Staff MapToDomain(Request request)
            {
                return new Staff
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    IsDeleted = false,
                    Gender = Enum.Parse<Enums.GenderType>(request.Gender)
                };
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                // Validate Name
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Staff name is required")
                    .MaximumLength(50).WithMessage("Student name must not exceed 50 characters");

                // Validate Email
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required")
                    .EmailAddress().WithMessage("Invalid email format");

                // Validate PhoneNumber
                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required");
                // Validate Gender
                RuleFor(x => x.Gender).IsEnumName(typeof(Enums.GenderType));
            }
        }
    }
}