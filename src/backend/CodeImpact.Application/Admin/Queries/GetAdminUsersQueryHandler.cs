using CodeImpact.Application.Admin.Dto;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, AdminUserListDto>
{
    private readonly IAdminUserDirectory _directory;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUserUsageSnapshotRepository _usageRepository;

    public GetAdminUsersQueryHandler(
        IAdminUserDirectory directory,
        IUserSubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository,
        IUserUsageSnapshotRepository usageRepository)
    {
        _directory = directory;
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _usageRepository = usageRepository;
    }

    public async Task<AdminUserListDto> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var users = await _directory.ListUsersAsync(request.Email, request.Status, page, pageSize);
        var totalCount = await _directory.CountUsersAsync(request.Email, request.Status);

        var items = new List<AdminUserListItemDto>(users.Count);
        foreach (var user in users)
        {
            var subscription = await _subscriptionRepository.GetByUserIdAsync(user.UserId);
            var plan = subscription is null ? null : await _planRepository.GetByIdAsync(subscription.PlanId);
            var usage = await _usageRepository.GetByUserIdAsync(user.UserId);

            items.Add(new AdminUserListItemDto(
                user.UserId,
                user.Email,
                user.FullName,
                user.AccountStatus,
                user.Roles,
                user.SupportFlags,
                user.CreatedAt,
                user.LastLoginAt,
                subscription?.Status,
                plan?.Name,
                usage?.RepositoriesUsed ?? 0,
                usage?.ReportsUsedThisMonth ?? 0));
        }

        return new AdminUserListDto(items, totalCount, page, pageSize);
    }
}