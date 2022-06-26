using Microsoft.AspNetCore.Identity.UI.Services;

namespace RecruitmentBackend.Services;

public class EmailSender: IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine("Email sent to: " + email);
        Console.WriteLine("With subject: " + subject);
        Console.WriteLine("And message: " + htmlMessage);
        return Task.CompletedTask;
    }
}
