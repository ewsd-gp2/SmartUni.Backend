namespace SmartUni.PublicApi.Features.Email
{
    public sealed class SendEmailRequestModel
    {
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
