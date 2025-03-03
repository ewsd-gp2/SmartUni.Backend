using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Student.Commands
{
    public class CreateStudent
    {
        private sealed record Request(string Name, string Email, string PhoneNumber, string Gender,string Major, Guid CreatedBy);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/student", HandleAsync)
                    .ProducesValidationProblem()
                    .WithTags(nameof(Student));
            }

            private static async Task<Results<Created<Student>, BadRequest<ValidationResult>>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                Request request,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to create a new student with request: {Request}", request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult);
                }

                Student student = MapToDomain(request);

                await dbContext.Student.AddAsync(student, cancellationToken);
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
                    IsDeleted = false,
                    Gender = Enum.Parse<Enums.GenderType>(request.Gender),
                    Major = Enum.Parse<Enums.MajorType>(request.Major),
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
                RuleFor(x => x.Gender)
                    .NotEmpty().WithMessage("Gender is required")
                    .Matches(@"^[M|F]$").WithMessage("Gender must be 'M' or 'F'");
                RuleFor(x => x.CreatedBy)
                    .NotEmpty().WithMessage("Created By is required");
            }
        }
    }
}