using Server.SharedKernel.MediatR;

namespace Server.UseCases.Config.Get;

public record GetConfigQuery : IQuery<ConfigDefinitions>;
