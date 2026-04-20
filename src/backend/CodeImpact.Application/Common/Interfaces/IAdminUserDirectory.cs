namespace CodeImpact.Application.Common.Interfaces;

public sealed record AdminDirectoryUser(
    Guid UserId,
    string Email,
    string FullName,
    string AccountStatus,
    string[] Roles,
    string[] SupportFlags,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public interface IAdminUserDirectory
{
    Task<IReadOnlyCollection<AdminDirectoryUser>> ListUsersAsync(string? emailFilter, string? statusFilter, int page, int pageSize);
    Task<int> CountUsersAsync(string? emailFilter, string? statusFilter);
    Task<AdminDirectoryUser?> GetByIdAsync(Guid userId);
    Task<bool> UpdateStatusAsync(Guid userId, string status);
    Task<bool> UpdateSupportFlagsAsync(Guid userId, string[] supportFlags);
}