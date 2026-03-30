using Server.Core.IdentityAggregate;
using Server.Web.Users.List;
using Server.Web.Users.UpdateRoles;

namespace Server.FunctionalTests.Users;

public class UpdateRolesTests : AppTestBase
{
  public UpdateRolesTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task UpdateRoles_AddAdminRole_Succeeds()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);

    var request = new UpdateRolesRequest
    {
      UserId = invitedUserId,
      Roles = [DefaultRoles.Admin, DefaultRoles.Author],
    };
    var (response, _) = await owner.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

    // Verify roles were updated
    var (_, listResult) = await owner.Client.GETAsync<ListUsers, UsersResponse>();
    var updatedUser = listResult.Users.First(u => u.Email == tenant.Users[1].Email);
    updatedUser.Roles.ShouldContain(DefaultRoles.Admin);
    updatedUser.Roles.ShouldContain(DefaultRoles.Author);
  }

  [Fact]
  public async Task UpdateRoles_RemoveRole_Succeeds()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);

    // First add ADMIN + AUTHOR
    var addRequest = new UpdateRolesRequest
    {
      UserId = invitedUserId,
      Roles = [DefaultRoles.Admin, DefaultRoles.Author],
    };
    await owner.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(addRequest);

    // Now set to AUTHOR only (removing ADMIN)
    var request = new UpdateRolesRequest
    {
      UserId = invitedUserId,
      Roles = [DefaultRoles.Author],
    };
    var (response, _) = await owner.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

    // Verify ADMIN was removed
    var (_, listResult) = await owner.Client.GETAsync<ListUsers, UsersResponse>();
    var updatedUser = listResult.Users.First(u => u.Email == tenant.Users[1].Email);
    updatedUser.Roles.ShouldNotContain(DefaultRoles.Admin);
    updatedUser.Roles.ShouldContain(DefaultRoles.Author);
  }

  [Fact]
  public async Task UpdateRoles_CannotAssignOwnerRole_ReturnsError()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);

    var request = new UpdateRolesRequest
    {
      UserId = invitedUserId,
      Roles = [DefaultRoles.Owner, DefaultRoles.Author],
    };
    var (response, _) = await owner.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateRoles_CannotRemoveOwnAdminRole_ReturnsError()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var owner = tenant.Users[0];
    var ownerId = await GetUserIdByEmail(owner.Client, owner.Email);

    var request = new UpdateRolesRequest
    {
      UserId = ownerId,
      Roles = [DefaultRoles.Author],
    };
    var (response, _) = await owner.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task UpdateRoles_OwnerRolePreserved_WhenNotInRequest()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var owner = tenant.Users[0];
    var ownerId = await GetUserIdByEmail(owner.Client, owner.Email);

    // Update roles to just ADMIN + AUTHOR (OWNER should be preserved automatically)
    var request = new UpdateRolesRequest
    {
      UserId = ownerId,
      Roles = [DefaultRoles.Admin, DefaultRoles.Author],
    };
    var (response, _) = await owner.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

    // Verify OWNER is still there
    var (_, listResult) = await owner.Client.GETAsync<ListUsers, UsersResponse>();
    var ownerUser = listResult.Users.First(u => u.Email == owner.Email);
    ownerUser.Roles.ShouldContain(DefaultRoles.Owner);
    ownerUser.Roles.ShouldContain(DefaultRoles.Admin);
    ownerUser.Roles.ShouldContain(DefaultRoles.Author);
  }

  [Fact]
  public async Task UpdateRoles_WithInvalidRole_ReturnsValidationError()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);

    var request = new UpdateRolesRequest
    {
      UserId = invitedUserId,
      Roles = ["INVALID_ROLE"],
    };
    var (response, _) = await owner.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateRoles_WithEmptyRoles_ReturnsValidationError()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);

    var request = new UpdateRolesRequest
    {
      UserId = invitedUserId,
      Roles = [],
    };
    var (response, _) = await owner.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateRoles_WithNonAdminRole_ReturnsForbidden()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var ownerId = await GetUserIdByEmail(owner.Client, owner.Email);

    var request = new UpdateRolesRequest
    {
      UserId = ownerId,
      Roles = [DefaultRoles.Author],
    };
    var (response, _) = await tenant.Users[1].Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task UpdateRoles_WithoutAuthentication_ReturnsUnauthorized()
  {
    var request = new UpdateRolesRequest
    {
      UserId = Guid.NewGuid(),
      Roles = [DefaultRoles.Author],
    };
    var (response, _) = await Fixture.Client.PUTAsync<UpdateRoles, UpdateRolesRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  private async Task<Guid> GetUserIdByEmail(HttpClient client, string email)
  {
    var (_, result) = await client.GETAsync<ListUsers, UsersResponse>();
    var user = result.Users.First(u => u.Email == email);
    return user.Id;
  }
}
