namespace SmartUni.PublicApi.Features.Email.Interface
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
