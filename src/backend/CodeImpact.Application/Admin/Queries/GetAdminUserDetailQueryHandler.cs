using CodeImpact.Application.Admin.Dto;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed class GetAdminUserDetailQueryHandler : IRequestHandler<GetAdminUserDetailQuery, AdminUserDetailDto?>
{
    private readonly IAdminUserDirectory _directory;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUserUsageSnapshotRepository _usageRepository;

    public GetAdminUserDetailQueryHandler(
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

    public async Task<AdminUserDetailDto?> Handle(GetAdminUserDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await _directory.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return null;
        }

        var subscription = await _subscriptionRepository.GetByUserIdAsync(user.UserId);
        var plan = subscription is null ? null : await _planRepository.GetByIdAsync(subscription.PlanId);
        var usage = await _usageRepository.GetByUserIdAsync(user.UserId);

        var detail = new AdminUserListItemDto(
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
            usage?.ReportsUsedThisMonth ?? 0);

        return new AdminUserDetailDto(detail, usage?.LastSyncAt);
    }
}