namespace Server.Web.DevOnly.UseCases.WipeData;

/// <summary>
/// Command to wipe all users and user-generated content from the database.
/// This is used for E2E test cleanup to ensure test isolation.
/// </summary>
public record WipeDataCommand() : Server.SharedKernel.MediatR.ICommand<Unit>;
