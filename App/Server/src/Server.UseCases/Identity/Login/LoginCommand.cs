using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Login;

public record LoginCommand(string Email, string Password, bool UseCookies, bool UseSessionCookies) : IQuery<LoginResult>;
