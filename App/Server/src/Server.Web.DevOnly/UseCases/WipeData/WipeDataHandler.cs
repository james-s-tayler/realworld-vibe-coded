using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;

namespace Server.Web.DevOnly.UseCases.WipeData;

/// <summary>
/// Handler that wipes all users from the database.
/// This handler deletes all data in the correct order to respect foreign key constraints.
/// </summary>
#pragma warning disable SRV015 // DevOnly test endpoint
#pragma warning disable PV051 // DevOnly - infrastructure access needed for bulk delete
#pragma warning disable PV002 // DevOnly - infrastructure access needed for bulk delete
#pragma warning disable PV014 // DevOnly - using ExecuteDeleteAsync instead of repository
public class WipeDataHandler(AppDbContext dbContext) : Server.SharedKernel.MediatR.ICommandHandler<WipeDataCommand, Unit>
{
  public async Task<Result<Unit>> Handle(WipeDataCommand request, CancellationToken cancellationToken)
  {
    // Delete users
    await dbContext.Users.ExecuteDeleteAsync(cancellationToken);

    return Result<Unit>.NoContent();
  }
}
#pragma warning restore PV014
#pragma warning restore PV002
#pragma warning restore PV051
#pragma warning restore SRV015
