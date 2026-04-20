using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed class UpdateAdminUserSubscriptionCommandHandler : IRequestHandler<UpdateAdminUserSubscriptionCommand, bool>
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IAdminAuditLogRepository _auditRepository;

    public UpdateAdminUserSubscriptionCommandHandler(
        IUserSubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository,
        IAdminAuditLogRepository auditRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(UpdateAdminUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(request.PlanId);
        if (plan is null)
        {
            await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
                request.AdminUserId,
                "UpdateUserSubscription",
                "User",
                request.UserId.ToString(),
                "plan-not-found",
                "failure",
                request.IpAddress));

            return false;
        }

        var subscription = await _subscriptionRepository.GetByUserIdAsync(request.UserId);
        if (subscription is null)
        {
            subscription = new UserSubscription(
                request.UserId,
                request.PlanId,
                request.Status,
                DateTime.UtcNow,
                request.CurrentPeriodEnd,
                request.AutoRenew,
                request.BillingIssue);
            await _subscriptionRepository.AddAsync(subscription);
        }
        else
        {
            subscription.UpdatePlan(request.PlanId);
            subscription.UpdateStatus(request.Status, request.BillingIssue);
            subscription.UpdateRenewal(request.AutoRenew, request.CurrentPeriodEnd);
            await _subscriptionRepository.UpdateAsync(subscription);
        }

        await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
            request.AdminUserId,
            "UpdateUserSubscription",
            "User",
            request.UserId.ToString(),
            $"planId={request.PlanId};status={request.Status};autoRenew={request.AutoRenew}",
            "success",
            request.IpAddress));

        return true;
    }
}