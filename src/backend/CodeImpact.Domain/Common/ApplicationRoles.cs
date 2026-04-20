namespace CodeImpact.Domain.Common;

public static class ApplicationRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Viewer = "Viewer";

    public static readonly string[] All = { Owner, Admin, Manager, Viewer };
}