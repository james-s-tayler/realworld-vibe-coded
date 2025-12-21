// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// SRV007: ApiFixtureBase.RegisterUserAsync uses raw HttpClient.PostAsJsonAsync to call ASP.NET Core Identity API
[assembly: SuppressMessage(
  "Server.Analyzers",
  "SRV007:Do not use raw HttpClient methods. Use FastEndpoints extension methods like POSTAsync, GETAsync, PUTAsync, DELETEAsync, or PATCHAsync instead for better test readability and consistency.",
  Justification = "Identity API register endpoint (MapIdentityApi) is not a FastEndpoints endpoint",
  Scope = "member",
  Target = "~M:Server.FunctionalTests.ApiFixtureBase`1.RegisterUserAsync(System.String,System.String,System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.String}")]

// SRV007: ApiFixtureBase.RegisterUserAndCreateClientAsync calls RegisterUserAsync which uses raw HttpClient
[assembly: SuppressMessage(
  "Server.Analyzers",
  "SRV007:Do not use raw HttpClient methods. Use FastEndpoints extension methods like POSTAsync, GETAsync, PUTAsync, DELETEAsync, or PATCHAsync instead for better test readability and consistency.",
  Justification = "Convenience method that internally calls RegisterUserAsync which uses Identity API",
  Scope = "member",
  Target = "~M:Server.FunctionalTests.ApiFixtureBase`1.RegisterUserAndCreateClientAsync(System.String,System.String,System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.ValueTuple{System.Net.Http.HttpClient,System.String,System.String}}")]

// SRV007: ApiFixtureBase.LoginUserAsync uses raw HttpClient.PostAsJsonAsync to call ASP.NET Core Identity API
[assembly: SuppressMessage(
  "Server.Analyzers",
  "SRV007:Do not use raw HttpClient methods. Use FastEndpoints extension methods like POSTAsync, GETAsync, PUTAsync, DELETEAsync, or PATCHAsync instead for better test readability and consistency.",
  Justification = "Identity API login endpoint (MapIdentityApi) is not a FastEndpoints endpoint",
  Scope = "member",
  Target = "~M:Server.FunctionalTests.ApiFixtureBase`1.LoginUserAsync(System.String,System.String,System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.String}")]
