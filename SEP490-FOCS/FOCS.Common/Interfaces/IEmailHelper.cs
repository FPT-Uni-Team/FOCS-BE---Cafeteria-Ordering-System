namespace FOCS.Common.Interfaces
{
    public interface IEmailHelper
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
