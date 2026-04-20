using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed record UpdateAdminUserSubscriptionCommand(
    Guid AdminUserId,
    Guid UserId,
    Guid PlanId,
    string Status,
    bool AutoRenew,
    DateTime CurrentPeriodEnd,
    string? BillingIssue,
    string? IpAddress) : IRequest<bool>;