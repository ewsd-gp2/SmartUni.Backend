using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Features.Email.Interface;

namespace SmartUni.PublicApi.Features.Email.Commands;

public class SendEmail
{
    private sealed record Request(SendEmailRequestModel Data);

    public sealed class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/send-email", HandleAsync)
                .ProducesValidationProblem()
                .WithTags("Email");
        }

        private static async Task<Results<Ok<string>, BadRequest<Dictionary<string, string[]>>>>
            HandleAsync(
                Request request,
                IEmailSender emailSender,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
        {
            logger.LogInformation("Email send request received: {Request}", request.Data);

            // Validate request
            SendEmailValidator validator = new();
            ValidationResult validationResult = await validator.ValidateAsync(request.Data, cancellationToken);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for email request: {Errors}", validationResult.Errors);
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return TypedResults.BadRequest(errors);
            }

            await emailSender.SendEmailAsync(request.Data.Email, request.Data.Subject, request.Data.Message);
            logger.LogInformation("Email sent to {Email}", request.Data.Email);

            return TypedResults.Ok("Email sent successfully.");
        }
    }
    public sealed class SendEmailValidator : AbstractValidator<SendEmailRequestModel>
    {
        public SendEmailValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required.");
            RuleFor(x => x.Subject).NotEmpty().WithMessage("Subject is required.");
            RuleFor(x => x.Message).NotEmpty().WithMessage("Message body is required.");
        }
    }
}
