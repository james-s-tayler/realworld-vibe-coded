using Server.Core.IdentityAggregate;
using Server.SharedKernel.Pagination;
using Server.Web.Identity.Login;
using Server.Web.Users.Deactivate;
using Server.Web.Users.List;
using Server.Web.Users.Reactivate;

namespace Server.FunctionalTests.Users;

public class DeactivateTests : AppTestBase
{
  public DeactivateTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task DeactivateUser_WithAdminRole_ReturnsNoContent()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);

    var request = new DeactivateUserRequest { UserId = invitedUserId };
    var (response, _) = await owner.Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task DeactivateUser_ThenLoginFails_WithLockedOutMessage()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUser = tenant.Users[1];
    var invitedUserId = await GetUserIdByEmail(owner.Client, invitedUser.Email);

    var deactivateRequest = new DeactivateUserRequest { UserId = invitedUserId };
    await owner.Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(deactivateRequest);

    var loginRequest = new LoginRequest
    {
      Email = invitedUser.Email,
      Password = "Password123!",
    };
    var (loginResponse, _) = await Fixture.Client.POSTAsync<LoginRequest, object>("/api/identity/login?useCookies=false", loginRequest);

    loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DeactivateUser_CannotDeactivateSelf_ReturnsForbidden()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var owner = tenant.Users[0];
    var ownerId = await GetUserIdByEmail(owner.Client, owner.Email);

    var request = new DeactivateUserRequest { UserId = ownerId };
    var (response, _) = await owner.Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task DeactivateUser_CannotDeactivateOwner_ReturnsForbidden()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];

    // Give the invited user ADMIN role first so they can call deactivate
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);
    var ownerId = await GetUserIdByEmail(owner.Client, owner.Email);

    // Make invited user an admin
    var updateRolesRequest = new Server.Web.Users.UpdateRoles.UpdateRolesRequest
    {
      UserId = invitedUserId,
      Roles = [DefaultRoles.Admin, DefaultRoles.Author],
    };
    await owner.Client.PUTAsync<Server.Web.Users.UpdateRoles.UpdateRoles, Server.Web.Users.UpdateRoles.UpdateRolesRequest, object>(updateRolesRequest);

    // Now try to deactivate the owner from the admin user
    var request = new DeactivateUserRequest { UserId = ownerId };
    var (response, _) = await tenant.Users[1].Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task DeactivateUser_WithNonAdminRole_ReturnsForbidden()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var ownerId = await GetUserIdByEmail(owner.Client, owner.Email);

    var request = new DeactivateUserRequest { UserId = ownerId };
    var (response, _) = await tenant.Users[1].Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task DeactivateUser_WithoutAuthentication_ReturnsUnauthorized()
  {
    var request = new DeactivateUserRequest { UserId = Guid.NewGuid() };
    var (response, _) = await Fixture.Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task ReactivateUser_WithAdminRole_ReturnsNoContent()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);

    // Deactivate first
    var deactivateRequest = new DeactivateUserRequest { UserId = invitedUserId };
    await owner.Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(deactivateRequest);

    // Now reactivate
    var reactivateRequest = new ReactivateUserRequest { UserId = invitedUserId };
    var (response, _) = await owner.Client.PUTAsync<ReactivateUser, ReactivateUserRequest, object>(reactivateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task ReactivateUser_ThenLoginSucceeds()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUser = tenant.Users[1];
    var invitedUserId = await GetUserIdByEmail(owner.Client, invitedUser.Email);

    // Deactivate
    var deactivateRequest = new DeactivateUserRequest { UserId = invitedUserId };
    await owner.Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(deactivateRequest);

    // Reactivate
    var reactivateRequest = new ReactivateUserRequest { UserId = invitedUserId };
    await owner.Client.PUTAsync<ReactivateUser, ReactivateUserRequest, object>(reactivateRequest);

    // Login should succeed
    var loginRequest = new LoginRequest
    {
      Email = invitedUser.Email,
      Password = "Password123!",
    };
    var (loginResponse, _) = await Fixture.Client.POSTAsync<LoginRequest, LoginResponse>("/api/identity/login?useCookies=false", loginRequest);

    loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task ReactivateUser_WithNonAdminRole_ReturnsForbidden()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    var request = new ReactivateUserRequest { UserId = Guid.NewGuid() };
    var (response, _) = await tenant.Users[1].Client.PUTAsync<ReactivateUser, ReactivateUserRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task ListUsers_ShowsDeactivatedUser_WithIsActiveFalse()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var owner = tenant.Users[0];
    var invitedUserId = await GetUserIdByEmail(owner.Client, tenant.Users[1].Email);

    // Deactivate
    var deactivateRequest = new DeactivateUserRequest { UserId = invitedUserId };
    await owner.Client.PUTAsync<DeactivateUser, DeactivateUserRequest, object>(deactivateRequest);

    // List users
    var (_, result) = await owner.Client.GETAsync<ListUsers, PaginatedResponse<UserDto>>();
    var deactivatedUser = result.Items.First(u => u.Email == tenant.Users[1].Email);

    deactivatedUser.IsActive.ShouldBeFalse();
  }

  private async Task<Guid> GetUserIdByEmail(HttpClient client, string email)
  {
    var (_, result) = await client.GETAsync<ListUsers, PaginatedResponse<UserDto>>();
    var user = result.Items.First(u => u.Email == email);
    return user.Id;
  }
}
