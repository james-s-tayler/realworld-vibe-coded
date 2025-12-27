using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Infrastructure.Data;

namespace Server.Infrastructure.Identity;

public class TenantClaimsTransformation : IClaimsTransformation
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly AppDbContext _dbContext;

  public TenantClaimsTransformation(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext)
  {
    _userManager = userManager;
    _dbContext = dbContext;
  }

  public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
  {
    // Check if TenantId claim already exists (idempotency)
    if (principal.HasClaim(c => c.Type == "TenantId"))
    {
      return principal;
    }

    // Get user ID from claims
    var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
    {
      return principal;
    }

    // Get user from database to access TenantId shadow property
    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null)
    {
      return principal;
    }

    // Get TenantId shadow property value
    var entry = _dbContext.Entry(user);
    var tenantId = entry.Property("TenantId").CurrentValue as string;

    if (!string.IsNullOrEmpty(tenantId))
    {
      // Clone the principal and add the TenantId claim
      var identity = (ClaimsIdentity)principal.Identity!;
      var claimsIdentity = new ClaimsIdentity(identity.Claims, identity.AuthenticationType);
      claimsIdentity.AddClaim(new Claim("TenantId", tenantId));

      return new ClaimsPrincipal(claimsIdentity);
    }

    return principal;
  }
}
