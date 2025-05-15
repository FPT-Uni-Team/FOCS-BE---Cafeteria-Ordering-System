namespace TipTrip.Common.Interfaces
{
    public interface IEmailHelper
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
