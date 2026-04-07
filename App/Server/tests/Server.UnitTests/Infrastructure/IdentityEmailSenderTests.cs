using Microsoft.Extensions.Logging.Abstractions;
using Server.Core.IdentityAggregate;
using Server.Infrastructure.Email;
using Server.UseCases.Interfaces;

namespace Server.UnitTests.Infrastructure;

public class IdentityEmailSenderTests
{
  private readonly IEmailSender _innerSender;
  private readonly IdentityEmailSender _sut;

  public IdentityEmailSenderTests()
  {
    _innerSender = Substitute.For<IEmailSender>();
    var logger = NullLogger<IdentityEmailSender>.Instance;
    _sut = new IdentityEmailSender(_innerSender, logger);
  }

  [Fact]
  public async Task SendConfirmationLinkAsync_CallsInnerSender()
  {
    var user = new ApplicationUser { Email = "test@example.com" };

    await _sut.SendConfirmationLinkAsync(user, "test@example.com", "https://confirm.link");

    await _innerSender.Received(1).SendEmailAsync(
      "test@example.com",
      "noreply@conduit.com",
      "Confirm your email",
      Arg.Is<string>(b => b.Contains("https://confirm.link")));
  }

  [Fact]
  public async Task SendConfirmationLinkAsync_WhenSenderThrows_DoesNotPropagate()
  {
    var user = new ApplicationUser { Email = "test@example.com" };
    _innerSender.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
      .Returns(Task.FromException(new InvalidOperationException("SMTP down")));

    await Should.NotThrowAsync(() => _sut.SendConfirmationLinkAsync(user, "test@example.com", "https://confirm.link"));
  }

  [Fact]
  public async Task SendPasswordResetLinkAsync_CallsInnerSender()
  {
    var user = new ApplicationUser { Email = "test@example.com" };

    await _sut.SendPasswordResetLinkAsync(user, "test@example.com", "https://reset.link");

    await _innerSender.Received(1).SendEmailAsync(
      "test@example.com",
      "noreply@conduit.com",
      "Reset your password",
      Arg.Is<string>(b => b.Contains("https://reset.link")));
  }

  [Fact]
  public async Task SendPasswordResetLinkAsync_WhenSenderThrows_DoesNotPropagate()
  {
    var user = new ApplicationUser { Email = "test@example.com" };
    _innerSender.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
      .Returns(Task.FromException(new InvalidOperationException("SMTP down")));

    await Should.NotThrowAsync(() => _sut.SendPasswordResetLinkAsync(user, "test@example.com", "https://reset.link"));
  }

  [Fact]
  public async Task SendPasswordResetCodeAsync_CallsInnerSender()
  {
    var user = new ApplicationUser { Email = "test@example.com" };

    await _sut.SendPasswordResetCodeAsync(user, "test@example.com", "123456");

    await _innerSender.Received(1).SendEmailAsync(
      "test@example.com",
      "noreply@conduit.com",
      "Reset your password",
      Arg.Is<string>(b => b.Contains("123456")));
  }

  [Fact]
  public async Task SendPasswordResetCodeAsync_WhenSenderThrows_DoesNotPropagate()
  {
    var user = new ApplicationUser { Email = "test@example.com" };
    _innerSender.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
      .Returns(Task.FromException(new InvalidOperationException("SMTP down")));

    await Should.NotThrowAsync(() => _sut.SendPasswordResetCodeAsync(user, "test@example.com", "123456"));
  }
}
