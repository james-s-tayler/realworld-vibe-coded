using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.UseCases.Interfaces;

namespace Server.Infrastructure.Email;

/// <summary>
/// Adapter that implements Microsoft.AspNetCore.Identity.IEmailSender for ApplicationUser
/// by wrapping the existing application IEmailSender service.
/// This is required by MapIdentityApi to send confirmation and password reset emails.
/// Uses IServiceProvider to resolve scoped IEmailSender at runtime since MapIdentityApi
/// requires a singleton service but IEmailSender is registered as scoped.
/// </summary>
public class IdentityEmailSender(IServiceProvider serviceProvider, ILogger<IdentityEmailSender> logger)
  : IEmailSender<ApplicationUser>
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly ILogger<IdentityEmailSender> _logger = logger;
  private const string DefaultFromAddress = "noreply@conduit.com";

  public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
  {
    _logger.LogInformation("Sending confirmation email to {Email} for user {UserId}", email, user.Id);
    var subject = "Confirm your email";
    var body = $"Please confirm your account by clicking this link: {confirmationLink}";

    using var scope = _serviceProvider.CreateScope();
    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
    await emailSender.SendEmailAsync(email, DefaultFromAddress, subject, body);
  }

  public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
  {
    _logger.LogInformation("Sending password reset email to {Email} for user {UserId}", email, user.Id);
    var subject = "Reset your password";
    var body = $"Please reset your password by clicking this link: {resetLink}";

    using var scope = _serviceProvider.CreateScope();
    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
    await emailSender.SendEmailAsync(email, DefaultFromAddress, subject, body);
  }

  public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
  {
    _logger.LogInformation("Sending password reset code to {Email} for user {UserId}", email, user.Id);
    var subject = "Reset your password";
    var body = $"Please reset your password using the following code: {resetCode}";

    using var scope = _serviceProvider.CreateScope();
    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
    await emailSender.SendEmailAsync(email, DefaultFromAddress, subject, body);
  }
}
