using CodeImpact.Application.Admin.Dto;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed class GetAdminUserSubscriptionQueryHandler : IRequestHandler<GetAdminUserSubscriptionQuery, AdminUserSubscriptionDto>
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;

    public GetAdminUserSubscriptionQueryHandler(
        IUserSubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
    }

    public async Task<AdminUserSubscriptionDto> Handle(GetAdminUserSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(request.UserId);
        var plans = await _planRepository.ListActiveAsync();
        var selectedPlan = subscription is null ? null : await _planRepository.GetByIdAsync(subscription.PlanId);

        var options = plans
            .Select(plan => new AdminPlanOptionDto(
                plan.Id,
                plan.Name,
                plan.Description,
                plan.RepositoriesLimit,
                plan.ReportsPerMonth,
                plan.RetentionDays,
                plan.IsActive))
            .ToArray();

        return new AdminUserSubscriptionDto(
            request.UserId,
            subscription?.Id,
            subscription?.PlanId,
            selectedPlan?.Name,
            subscription?.Status ?? UserSubscriptionStatus.Trial,
            subscription?.CurrentPeriodStart,
            subscription?.CurrentPeriodEnd,
            subscription?.AutoRenew ?? false,
            subscription?.BillingIssue,
            options);
    }
}