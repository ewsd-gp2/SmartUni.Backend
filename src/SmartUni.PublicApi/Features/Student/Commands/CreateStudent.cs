using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Student.Commands
{
    public class CreateStudent
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
                endpoints.MapPost("/student", HandleAsync)
                    .ProducesValidationProblem()
                    .RequireAuthorization("api")
                    .WithDescription("Create new student")
                    .Accepts<Request>("multipart/form-data")
                    .Produces(201)
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .WithTags(nameof(Student))
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
                logger.LogInformation("Submitted to create a new student with request: {Request}", request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors
                        .Select(e => new { e.PropertyName, e.ErrorMessage }).ToList());
                }

                Student student = MapToDomain(request);
                student.CreatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                               throw new InvalidOperationException(ClaimTypes.NameIdentifier));
                BaseUser user = new()
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    IsFirstLogin = true,
                    Student = student,
                    Role = Enums.RoleType.Student
                };

                IdentityResult result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Failed to create user: {Errors}", result.Errors);
                    return TypedResults.BadRequest(result.Errors.Select(e => new { e.Code, e.Description }).ToList());
                }

                //dbContext.Student.Add(student);
                //await dbContext.SaveChangesAsync(cancellationToken); 

                logger.LogInformation("Successfully created a new student with ID: {Id}", student.Id);
                return TypedResults.Created($"/student/{student.Id}", student);
            }

            private static Student MapToDomain(Request request)
            {
                return new Student
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Gender = Enum.Parse<Enums.GenderType>(request.Gender),
                    Major = Enum.Parse<Enums.MajorType>(request.Major),
                    Image = GetFileArray(request.Image)
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
                RuleFor(x => x.Major).IsEnumName(typeof(Enums.MajorType));
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