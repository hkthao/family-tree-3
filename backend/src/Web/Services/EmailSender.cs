using System.Threading.Tasks;
using backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace backend.Web.Services;

public class EmailSender : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        // This is a dummy implementation. In a real application, you would send an email.
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        // This is a dummy implementation. In a real application, you would send an email.
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        // This is a dummy implementation. In a real application, you would send an email.
        return Task.CompletedTask;
    }
}
