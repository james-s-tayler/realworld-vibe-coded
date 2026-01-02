// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// SRV007: UsersTests.UpdateUser_WithNewPassword_CanLoginWithNewPassword uses raw HttpClient.PostAsJsonAsync to call ASP.NET Core Identity API
[assembly: SuppressMessage(
  "Server.Analyzers",
  "SRV007:Do not use raw HttpClient methods. Use FastEndpoints extension methods like POSTAsync, GETAsync, PUTAsync, DELETEAsync, or PATCHAsync instead for better test readability and consistency.",
  Justification = "Identity API login endpoint (MapIdentityApi) is not a FastEndpoints endpoint. Using raw HttpClient to test password change by verifying successful login with new password.",
  Scope = "member",
  Target = "~M:Server.FunctionalTests.Users.UsersTests.UpdateUser_WithNewPassword_CanLoginWithNewPassword~System.Threading.Tasks.Task")]
