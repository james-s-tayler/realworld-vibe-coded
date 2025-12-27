namespace Server.UseCases.Identity.Login;

public record LoginResult(string AccessToken, int ExpiresIn, string RefreshToken);
