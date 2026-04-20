using System.Text.Json;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Services;

public sealed class AdminUserDirectory : IAdminUserDirectory
{
    private readonly UserManager<AppUser> _userManager;

    public AdminUserDirectory(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyCollection<AdminDirectoryUser>> ListUsersAsync(string? emailFilter, string? statusFilter, int page, int pageSize)
    {
        var query = ApplyFilters(_userManager.Users.AsNoTracking(), emailFilter, statusFilter)
            .OrderBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var users = await query.ToListAsync();
        return await MapUsersAsync(users);
    }

    public Task<int> CountUsersAsync(string? emailFilter, string? statusFilter)
    {
        return ApplyFilters(_userManager.Users.AsNoTracking(), emailFilter, statusFilter)
            .CountAsync();
    }

    public async Task<AdminDirectoryUser?> GetByIdAsync(Guid userId)
    {
        var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == userId);
        if (user is null)
        {
            return null;
        }

        return (await MapUsersAsync(new[] { user })).Single();
    }

    public async Task<bool> UpdateStatusAsync(Guid userId, string status)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        user.AccountStatus = status;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateSupportFlagsAsync(Guid userId, string[] supportFlags)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        user.SupportFlagsJson = JsonSerializer.Serialize(supportFlags.Distinct().ToArray());
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    private async Task<IReadOnlyCollection<AdminDirectoryUser>> MapUsersAsync(IReadOnlyCollection<AppUser> users)
    {
        var results = new List<AdminDirectoryUser>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            results.Add(new AdminDirectoryUser(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                user.AccountStatus,
                roles.ToArray(),
                ParseSupportFlags(user.SupportFlagsJson),
                DateTime.UtcNow,
                null));
        }

        return results;
    }

    private static IQueryable<AppUser> ApplyFilters(IQueryable<AppUser> query, string? emailFilter, string? statusFilter)
    {
        if (!string.IsNullOrWhiteSpace(emailFilter))
        {
            query = query.Where(user => user.Email != null && user.Email.Contains(emailFilter));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(user => user.AccountStatus == statusFilter);
        }

        return query;
    }

    private static string[] ParseSupportFlags(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}