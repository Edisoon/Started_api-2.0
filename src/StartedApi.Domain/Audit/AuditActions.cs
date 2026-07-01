namespace StartedApi.Domain.Audit;

public static class AuditActions
{
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string PasswordChanged = "PasswordChanged";
    public const string PasswordReset = "PasswordReset";
    public const string UserUpdated = "UserUpdated";
    public const string UserStatusChanged = "UserStatusChanged";
    public const string RoleCreated = "RoleCreated";
    public const string RoleUpdated = "RoleUpdated";
    public const string RoleStatusChanged = "RoleStatusChanged";
    public const string RoleAssigned = "RoleAssigned";
    public const string RoleRemoved = "RoleRemoved";
    public const string AccountLocked = "AccountLocked";
}
