# Started API Auth Base Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a reusable ASP.NET Core 8 REST API base for users, authentication, authorization by roles, refresh tokens, audit logs, documentation, and basic automated tests.

**Architecture:** The solution uses practical Clean Architecture with `Domain`, `Application`, `Infrastructure`, `Api`, and `Tests` projects. ASP.NET Core Identity handles users, roles, password hashing, lockout, email confirmation tokens, and password reset tokens. JWT access tokens are short lived, while refresh tokens are stored hashed and rotated on use.

**Tech Stack:** ASP.NET Core 8, C#, Entity Framework Core, SQL Server, ASP.NET Core Identity, JWT Bearer Authentication, Swagger/OpenAPI, xUnit, WebApplicationFactory, SQLite in-memory for integration tests.

---

## File Structure

- Create: `StartedApi.sln` - solution file.
- Create: `src/StartedApi.Domain/StartedApi.Domain.csproj` - domain entities and constants.
- Create: `src/StartedApi.Application/StartedApi.Application.csproj` - DTOs, interfaces, service contracts, result types.
- Create: `src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj` - EF Core, Identity, token services, audit persistence.
- Create: `src/StartedApi.Api/StartedApi.Api.csproj` - REST controllers, middleware, Swagger, DI composition.
- Create: `tests/StartedApi.Tests/StartedApi.Tests.csproj` - service and API tests.
- Create: `README.md` - setup and usage documentation.
- Create: `StartedApi.Api.http` - executable HTTP examples.
- Create: `docs/architecture.md` - architecture explanation.
- Create: `docs/security.md` - security decisions.
- Create: `docs/api-reference.md` - endpoint reference.

## Task 1: Scaffold Solution and Projects

**Files:**
- Create: `StartedApi.sln`
- Create: `src/StartedApi.Domain/StartedApi.Domain.csproj`
- Create: `src/StartedApi.Application/StartedApi.Application.csproj`
- Create: `src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj`
- Create: `src/StartedApi.Api/StartedApi.Api.csproj`
- Create: `tests/StartedApi.Tests/StartedApi.Tests.csproj`

- [ ] **Step 1: Create solution and projects**

Run:

```powershell
dotnet new sln -n StartedApi
dotnet new classlib -n StartedApi.Domain -o src/StartedApi.Domain -f net8.0
dotnet new classlib -n StartedApi.Application -o src/StartedApi.Application -f net8.0
dotnet new classlib -n StartedApi.Infrastructure -o src/StartedApi.Infrastructure -f net8.0
dotnet new webapi -n StartedApi.Api -o src/StartedApi.Api -f net8.0 --use-controllers
dotnet new xunit -n StartedApi.Tests -o tests/StartedApi.Tests -f net8.0
dotnet sln StartedApi.sln add src/StartedApi.Domain/StartedApi.Domain.csproj
dotnet sln StartedApi.sln add src/StartedApi.Application/StartedApi.Application.csproj
dotnet sln StartedApi.sln add src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj
dotnet sln StartedApi.sln add src/StartedApi.Api/StartedApi.Api.csproj
dotnet sln StartedApi.sln add tests/StartedApi.Tests/StartedApi.Tests.csproj
```

Expected: solution and five projects are created.

- [ ] **Step 2: Add project references**

Run:

```powershell
dotnet add src/StartedApi.Application/StartedApi.Application.csproj reference src/StartedApi.Domain/StartedApi.Domain.csproj
dotnet add src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj reference src/StartedApi.Application/StartedApi.Application.csproj
dotnet add src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj reference src/StartedApi.Domain/StartedApi.Domain.csproj
dotnet add src/StartedApi.Api/StartedApi.Api.csproj reference src/StartedApi.Application/StartedApi.Application.csproj
dotnet add src/StartedApi.Api/StartedApi.Api.csproj reference src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj
dotnet add tests/StartedApi.Tests/StartedApi.Tests.csproj reference src/StartedApi.Api/StartedApi.Api.csproj
dotnet add tests/StartedApi.Tests/StartedApi.Tests.csproj reference src/StartedApi.Application/StartedApi.Application.csproj
dotnet add tests/StartedApi.Tests/StartedApi.Tests.csproj reference src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj
```

Expected: references match Clean Architecture dependency direction.

- [ ] **Step 3: Add NuGet packages**

Run:

```powershell
dotnet add src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/StartedApi.Infrastructure/StartedApi.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/StartedApi.Api/StartedApi.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/StartedApi.Api/StartedApi.Api.csproj package Swashbuckle.AspNetCore
dotnet add tests/StartedApi.Tests/StartedApi.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/StartedApi.Tests/StartedApi.Tests.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet add tests/StartedApi.Tests/StartedApi.Tests.csproj package FluentAssertions
```

Expected: packages restore successfully.

- [ ] **Step 4: Build**

Run:

```powershell
dotnet build StartedApi.sln
```

Expected: build succeeds.

## Task 2: Create Domain Models and Constants

**Files:**
- Create: `src/StartedApi.Domain/Users/ApplicationUser.cs`
- Create: `src/StartedApi.Domain/Roles/ApplicationRole.cs`
- Create: `src/StartedApi.Domain/Auth/RefreshToken.cs`
- Create: `src/StartedApi.Domain/Audit/AuditLog.cs`
- Create: `src/StartedApi.Domain/Security/AppRoles.cs`
- Create: `src/StartedApi.Domain/Audit/AuditActions.cs`

- [ ] **Step 1: Add Identity user**

Create `ApplicationUser`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace StartedApi.Domain.Users;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
```

- [ ] **Step 2: Add role, refresh token, and audit entities**

Create `ApplicationRole`, `RefreshToken`, and `AuditLog` using `Guid` identifiers, UTC dates, and no sensitive plain-text token fields.

- [ ] **Step 3: Add constants**

Create constants:

```csharp
namespace StartedApi.Domain.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
}
```

- [ ] **Step 4: Build**

Run:

```powershell
dotnet build StartedApi.sln
```

Expected: build succeeds.

## Task 3: Add Application Contracts, DTOs, and Result Types

**Files:**
- Create: `src/StartedApi.Application/Common/ApiResponse.cs`
- Create: `src/StartedApi.Application/Common/PagedResponse.cs`
- Create: `src/StartedApi.Application/Common/OperationResult.cs`
- Create: `src/StartedApi.Application/Auth/*.cs`
- Create: `src/StartedApi.Application/Users/*.cs`
- Create: `src/StartedApi.Application/Roles/*.cs`
- Create: `src/StartedApi.Application/Security/ICurrentUserService.cs`
- Create: `src/StartedApi.Application/Security/ITokenService.cs`
- Create: `src/StartedApi.Application/Audit/IAuditService.cs`

- [ ] **Step 1: Write DTOs before services**

Create request/response DTOs matching the approved endpoint table:

```csharp
namespace StartedApi.Application.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    UserProfileResponse User);
```

- [ ] **Step 2: Add service interfaces**

Define interfaces:

```csharp
public interface IAuthService
{
    Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<OperationResult<AuthMessageResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
}
```

Add remaining methods for confirm email, logout, refresh token, revoke token, forgot password, reset password, and change password.

- [ ] **Step 3: Build**

Run:

```powershell
dotnet build StartedApi.sln
```

Expected: build succeeds.

## Task 4: Configure Infrastructure Persistence and Identity

**Files:**
- Create: `src/StartedApi.Infrastructure/Persistence/ApplicationDbContext.cs`
- Create: `src/StartedApi.Infrastructure/Persistence/DependencyInjection.cs`
- Create: `src/StartedApi.Infrastructure/Identity/IdentitySeeder.cs`
- Modify: `src/StartedApi.Api/appsettings.json`
- Modify: `src/StartedApi.Api/Program.cs`

- [ ] **Step 1: Create DbContext**

Create an Identity DbContext:

```csharp
public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
}
```

- [ ] **Step 2: Configure entity mappings**

Configure indexes for `RefreshToken.TokenHash`, `AuditLog.OccurredAtUtc`, and `AuditLog.UserId`.

- [ ] **Step 3: Register Infrastructure services**

Add:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

- [ ] **Step 4: Build**

Run:

```powershell
dotnet build StartedApi.sln
```

Expected: build succeeds.

## Task 5: Implement Security and Token Services with Tests First

**Files:**
- Create: `tests/StartedApi.Tests/Security/RefreshTokenHasherTests.cs`
- Create: `src/StartedApi.Application/Security/IRefreshTokenHasher.cs`
- Create: `src/StartedApi.Infrastructure/Security/RefreshTokenHasher.cs`
- Create: `src/StartedApi.Infrastructure/Authentication/JwtOptions.cs`
- Create: `src/StartedApi.Infrastructure/Authentication/TokenService.cs`

- [ ] **Step 1: Write failing hasher test**

```csharp
[Fact]
public void Hash_ReturnsSameHash_ForSameToken()
{
    var hasher = new RefreshTokenHasher();

    var first = hasher.Hash("token-value");
    var second = hasher.Hash("token-value");

    first.Should().Be(second);
    first.Should().NotBe("token-value");
}
```

Run:

```powershell
dotnet test tests/StartedApi.Tests/StartedApi.Tests.csproj --filter RefreshTokenHasherTests
```

Expected: fail because `RefreshTokenHasher` does not exist.

- [ ] **Step 2: Implement minimal hasher**

Use SHA-256 for deterministic token lookup hashing:

```csharp
public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
```

- [ ] **Step 3: Run test**

Run:

```powershell
dotnet test tests/StartedApi.Tests/StartedApi.Tests.csproj --filter RefreshTokenHasherTests
```

Expected: test passes.

## Task 6: Implement Auth Service and Auth Controller

**Files:**
- Create: `tests/StartedApi.Tests/Auth/AuthEndpointsTests.cs`
- Create: `src/StartedApi.Infrastructure/Auth/AuthService.cs`
- Create: `src/StartedApi.Api/Controllers/AuthController.cs`
- Modify: `src/StartedApi.Infrastructure/Persistence/DependencyInjection.cs`

- [ ] **Step 1: Write failing registration endpoint test**

Create a test that posts to `/api/auth/register` and asserts successful registration response without sensitive fields.

- [ ] **Step 2: Implement registration**

Use `UserManager<ApplicationUser>.CreateAsync(user, password)` and assign default `User` role.

- [ ] **Step 3: Write failing login endpoint test**

Create a test that registers a confirmed user, posts to `/api/auth/login`, and expects access and refresh tokens.

- [ ] **Step 4: Implement login**

Use `SignInManager.CheckPasswordSignInAsync`, update `LastLoginAtUtc`, generate JWT, create refresh token hash, and audit login.

- [ ] **Step 5: Implement remaining auth endpoints**

Implement confirm email, logout, refresh token, revoke token, forgot password, reset password, and change password using existing Identity APIs.

- [ ] **Step 6: Run auth tests**

Run:

```powershell
dotnet test tests/StartedApi.Tests/StartedApi.Tests.csproj --filter AuthEndpointsTests
```

Expected: auth endpoint tests pass.

## Task 7: Implement Users Module

**Files:**
- Create: `tests/StartedApi.Tests/Users/UserEndpointsTests.cs`
- Create: `src/StartedApi.Infrastructure/Users/UserService.cs`
- Create: `src/StartedApi.Api/Controllers/UsersController.cs`

- [ ] **Step 1: Write failing admin authorization test**

Assert that a non-admin authenticated user receives `403 Forbidden` for `GET /api/users`.

- [ ] **Step 2: Implement profile endpoints**

Implement `GET /api/users/me` and `PUT /api/users/me` using current user id.

- [ ] **Step 3: Implement admin endpoints**

Implement list, get by id, and status update with `[Authorize(Roles = AppRoles.Admin)]`.

- [ ] **Step 4: Run user tests**

Run:

```powershell
dotnet test tests/StartedApi.Tests/StartedApi.Tests.csproj --filter UserEndpointsTests
```

Expected: user endpoint tests pass.

## Task 8: Implement Roles Module

**Files:**
- Create: `tests/StartedApi.Tests/Roles/RoleEndpointsTests.cs`
- Create: `src/StartedApi.Infrastructure/Roles/RoleService.cs`
- Create: `src/StartedApi.Api/Controllers/RolesController.cs`

- [ ] **Step 1: Write failing role assignment test**

Assert that an admin can assign `User` role to an existing user and then retrieve that role from `/api/roles/users/{userId}`.

- [ ] **Step 2: Implement role service**

Use `RoleManager<ApplicationRole>` and `UserManager<ApplicationUser>` for create, list, assign, remove, and get roles.

- [ ] **Step 3: Add audit events**

Audit role creation, assignment, and removal.

- [ ] **Step 4: Run role tests**

Run:

```powershell
dotnet test tests/StartedApi.Tests/StartedApi.Tests.csproj --filter RoleEndpointsTests
```

Expected: role endpoint tests pass.

## Task 9: Add Central Error Handling, Swagger, and API Consistency

**Files:**
- Create: `src/StartedApi.Api/Middleware/ExceptionHandlingMiddleware.cs`
- Create: `src/StartedApi.Api/Extensions/SwaggerExtensions.cs`
- Modify: `src/StartedApi.Api/Program.cs`

- [ ] **Step 1: Add exception middleware**

Return consistent JSON responses for validation errors, business errors, unauthorized access, and unexpected errors.

- [ ] **Step 2: Configure Swagger Bearer auth**

Add OpenAPI security scheme named `Bearer`.

- [ ] **Step 3: Run build**

Run:

```powershell
dotnet build StartedApi.sln
```

Expected: build succeeds.

## Task 10: Add EF Core Migration and Runtime Configuration

**Files:**
- Modify: `src/StartedApi.Api/appsettings.json`
- Create: `src/StartedApi.Api/appsettings.Development.json`
- Create: `src/StartedApi.Infrastructure/Persistence/Migrations/*`

- [ ] **Step 1: Add safe local development configuration**

Use local development values that must be overridden with user secrets or environment variables outside development:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StartedApiDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "StartedApi",
    "Audience": "StartedApiClients",
    "Secret": "DevelopmentOnly-Minimum32Characters-Key",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  }
}
```

- [ ] **Step 2: Create initial migration**

Run:

```powershell
dotnet ef migrations add InitialIdentityAuthSchema --project src/StartedApi.Infrastructure --startup-project src/StartedApi.Api
```

Expected: migration files are created.

- [ ] **Step 3: Build**

Run:

```powershell
dotnet build StartedApi.sln
```

Expected: build succeeds.

## Task 11: Add Documentation and HTTP Examples

**Files:**
- Create: `README.md`
- Create: `docs/architecture.md`
- Create: `docs/security.md`
- Create: `docs/api-reference.md`
- Create: `StartedApi.Api.http`

- [ ] **Step 1: Write README**

Include objective, requirements, setup, configuration, migrations, running the API, running tests, and token usage.

- [ ] **Step 2: Write architecture and security docs**

Summarize Clean Architecture boundaries, Identity decision, JWT/refresh token flow, and future extensions.

- [ ] **Step 3: Add HTTP examples**

Include register, confirm email, login, refresh token, profile, users, roles, and role assignment requests.

## Task 12: Full Verification

**Files:**
- Read: all created source, test, and documentation files.

- [ ] **Step 1: Run format**

Run:

```powershell
dotnet format StartedApi.sln
```

Expected: formatting completes without errors.

- [ ] **Step 2: Run build**

Run:

```powershell
dotnet build StartedApi.sln
```

Expected: build succeeds.

- [ ] **Step 3: Run tests**

Run:

```powershell
dotnet test StartedApi.sln
```

Expected: all tests pass.

- [ ] **Step 4: Run API locally**

Run:

```powershell
dotnet run --project src/StartedApi.Api
```

Expected: API starts and Swagger is available at `/swagger`.

## Scope Review

This plan implements the approved first version. It intentionally excludes 2FA, granular permissions, complete device management, multi-tenant behavior, and production email provider integration. Those items remain documented as future extensions.
