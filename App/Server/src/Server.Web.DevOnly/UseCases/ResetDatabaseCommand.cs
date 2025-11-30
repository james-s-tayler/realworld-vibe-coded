namespace Server.Web.DevOnly.UseCases;

public record ResetDatabaseCommand() : SharedKernel.MediatR.ICommand<Unit>;
