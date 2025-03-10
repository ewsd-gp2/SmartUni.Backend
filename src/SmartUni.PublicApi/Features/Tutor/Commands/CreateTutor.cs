using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Persistence;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SmartUni.PublicApi.Features.Tutor.Commands
{
    public sealed class CreateTutor
    {
        private sealed record Request(
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            string Major);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/tutor", HandleAsync)
                    .WithDescription("Create new tutor")
                    .Accepts<Request>("application/json")
                    .Produces(201)
                    .Produces<BadRequest<List<ValidationFailure>>>()
                    .WithTags(nameof(Tutor));
            }

            private static async Task<Results<Created, BadRequest<List<ValidationFailure>>>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                Request request,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to create a new tutor with request: {Request}", request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }

                Tutor tutor = MapToDomain(request);

                await dbContext.Tutor.AddAsync(tutor, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully created a new tutor with ID: {Id}", tutor.Id);
                return TypedResults.Created();
            }

            private static Tutor MapToDomain(Request request)
            {
                Tutor tutor = new()
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Gender = Enum.Parse<Enums.GenderType>(request.Gender),
                    Major = Enum.Parse<Enums.MajorType>(request.Major)
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
    }
}