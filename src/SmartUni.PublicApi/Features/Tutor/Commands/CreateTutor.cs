using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Persistence;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SmartUni.PublicApi.Features.Tutor.Commands
{
    public sealed class CreateTutor
    {
        private sealed record Request(string Name, string Email, string PhoneNumber);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/tutor", HandleAsync)
                    .ProducesValidationProblem()
                    .WithTags(nameof(Tutor));
            }

            private static async Task<Results<Created, BadRequest<ValidationResult>>> HandleAsync(
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
                    return TypedResults.BadRequest(validationResult);
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
                    PhoneNumber = request.PhoneNumber
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
            }
        }
    }
}