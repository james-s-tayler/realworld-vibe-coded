using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Infrastructure.Data;
using Server.UseCases.Identity;

namespace Server.Infrastructure.Identity;

public class TenantAssigner : ITenantAssigner
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly AppDbContext _dbContext;

  public TenantAssigner(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext)
  {
    _userManager = userManager;
    _dbContext = dbContext;
  }

  public async Task SetTenantIdAsync(Guid userId, string tenantIdentifier, CancellationToken cancellationToken = default)
  {
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
      throw new InvalidOperationException($"User with ID {userId} not found");
    }

    // Set TenantId shadow property
    var entry = _dbContext.Entry(user);
    entry.Property("TenantId").CurrentValue = tenantIdentifier;
    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
