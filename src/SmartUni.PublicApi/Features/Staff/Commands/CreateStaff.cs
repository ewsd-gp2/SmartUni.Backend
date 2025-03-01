using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Persistence;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SmartUni.PublicApi.Features.Staff.Commands
{
    public class CreateStaff
    {
        private sealed record Request(string Name, string Email, string PhoneNumber, string Gender, Guid CreatedBy);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/staff", HandleAsync)
                    .ProducesValidationProblem()
                    .WithTags(nameof(Staff));
            }

            private static async Task<Results<Created<Staff>, BadRequest<ValidationResult>>> HandleAsync(
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

                await dbContext.Staff.AddAsync(staff, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully created a new staff with ID: {Id}", staff.Id);

                return TypedResults.Created($"/student/{staff.Id}", staff);
            }

            private static Staff MapToDomain(Request request)
            {
                return new Staff
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    IsDeleted = false,
                    Gender = request.Gender,
                    CreatedBy = request.CreatedBy
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
                RuleFor(x => x.Gender)
                    .NotEmpty().WithMessage("Gender is required")
                    .Matches(@"^[M|F]$").WithMessage("Gender must be 'M' or 'F'");
                RuleFor(x => x.CreatedBy)
                    .NotEmpty().WithMessage("Created By is required");
            }
        }
    }
}