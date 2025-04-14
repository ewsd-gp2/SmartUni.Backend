using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Features.Email.Interface;

namespace SmartUni.PublicApi.Features.Email.Commands;

public class SendEmail
{
    public sealed record TutoringAllocationRequest(
        string StudentEmail,
        string StudentName,
        string TutorEmail,
        string TutorName,
        string Subject,
        string HelpCenterUrl,
        string senderName,
        string WebsiteUrl
    );

    private sealed record Request(TutoringAllocationRequest Data);

    public sealed class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/email", HandleAsync)
                .ProducesValidationProblem()
                .WithTags("Email")
                .WithName("SendTutoringAllocationEmail");
        }

        private static async Task<Results<Ok<string>, BadRequest<Dictionary<string, string[]>>>>
            HandleAsync(
                Request request,
                IEmailSender emailSender,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
        {
            logger.LogInformation("Preparing tutoring notification for {Student}", request.Data.StudentName);

            var validator = new TutoringAllocationValidator();
            var validationResult = await validator.ValidateAsync(request.Data, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                return TypedResults.BadRequest(errors);
            }
           
            var (subject, body) = BuildEmailContentForStudent(request.Data);

            await emailSender.SendEmailAsync(
                request.Data.StudentEmail,
                subject,
                body);
            (subject, body) = BuildEmailContentForTutor(request.Data);

            await emailSender.SendEmailAsync(
                request.Data.TutorEmail,
                subject,
                body);

            return TypedResults.Ok("Tutoring notification sent successfully.");
        }

        private static (string Subject, string Body) BuildEmailContentForStudent(TutoringAllocationRequest request)
        {
            var subject = $"Notification for Allocation ";

            var body = $"""
                Dear {request.StudentName},

                We’re excited to inform you that your "{request.Subject}" tutoring session with Tutor "{request.TutorName}" has been successfully scheduled!

                If you have any questions or need assistance, feel free to reach out to us at smartuniewsd@gmail.com.

                Looking forward to a great session!

                Best regards,  
                {request.senderName}
                SmartUni Admin Team  
                🌐 {request.WebsiteUrl}
                """;

            return (subject, body);
        }
        private static (string Subject, string Body) BuildEmailContentForTutor(TutoringAllocationRequest request)
        {
            var subject = $"Notification for Allocation";

            var body = $"""
                Dear {request.TutorName},

                You’ve been successfully assigned as a tutor for {request.StudentName}. Further details regarding sessions will be shared soon.

                If you have any questions or need assistance, feel free to reach out to us at smartuniewsd@gmail.com.

                Looking forward to a great session!

                Best regards,  
                {request.senderName}
                SmartUni Admin Team  
                🌐 {request.WebsiteUrl}
                """;

            return (subject, body);
        }
    }

    public sealed class TutoringAllocationValidator : AbstractValidator<TutoringAllocationRequest>
    {
        public TutoringAllocationValidator()
        {
            RuleFor(x => x.StudentEmail)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Valid student email is required.");

            RuleFor(x => x.StudentName)
                .NotEmpty()
                .WithMessage("Student name is required.");

            RuleFor(x => x.TutorName)
                .NotEmpty()
                .WithMessage("Tutor name is required.");

            RuleFor(x => x.Subject)
                .NotEmpty()
                .WithMessage("Subject is required.");

        }
    }
}