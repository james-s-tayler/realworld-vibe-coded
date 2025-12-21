// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// SRV007: ApiFixtureBase uses raw HttpClient methods to call ASP.NET Core Identity API endpoints
// which are not FastEndpoints. This is necessary and intentional.
[assembly: SuppressMessage(
  "Server.Analyzers",
  "SRV007:Do not use raw HttpClient methods. Use FastEndpoints extension methods like POSTAsync, GETAsync, PUTAsync, DELETEAsync, or PATCHAsync instead for better test readability and consistency.",
  Justification = "ApiFixtureBase calls ASP.NET Core Identity API endpoints (MapIdentityApi) which are not FastEndpoints",
  Scope = "namespaceanddescendants",
  Target = "~N:Server.FunctionalTests")]
