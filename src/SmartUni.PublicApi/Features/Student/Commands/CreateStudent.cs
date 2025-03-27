using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Student.Commands
{
    public class CreateStudent
    {
        private sealed record Request(string Name, string Email, string PhoneNumber, string Gender, string Major, string Password);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/student", HandleAsync)
                    .ProducesValidationProblem()
                    .RequireAuthorization("api")
                    .WithDescription("Create new student")
                    .Accepts<Request>("application/json")
                    .Produces(201)
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .WithTags(nameof(Student));
            }

            private static async Task<IResult> HandleAsync(
    ClaimsPrincipal claims,
    ILogger<Endpoint> logger,
    SmartUniDbContext dbContext,
    Request request,
    UserManager<BaseUser> userManager,
    CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to create a new student with request: {Request}", request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList());
                }

                BaseUser user = new()
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber
                };

                IdentityResult result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Failed to create user: {Errors}", result.Errors);
                    return TypedResults.BadRequest(result.Errors.Select(e => new { e.Code, e.Description }).ToList());
                }

                
                Student student = MapToDomain(request);
                student.CreatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    throw new InvalidOperationException(ClaimTypes.NameIdentifier));

                student.IdentityId = user.Id;

                dbContext.Student.Add(student);
                await dbContext.SaveChangesAsync(cancellationToken); 

                logger.LogInformation("Successfully created a new student with ID: {Id}", student.Id);
                return TypedResults.Created($"/student/{student.Id}", student);
            }
            private static Student MapToDomain(Request request)
            {
                return new Student
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Gender = Enum.Parse<Enums.GenderType>(request.Gender),
                    Major = Enum.Parse<Enums.MajorType>(request.Major)
                };
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                // Validate Name
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Student name is required")
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