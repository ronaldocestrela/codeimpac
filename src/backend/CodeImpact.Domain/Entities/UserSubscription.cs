using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public sealed class UserSubscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }
    public string Status { get; private set; } = UserSubscriptionStatus.Trial;
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public bool AutoRenew { get; private set; }
    public string? BillingIssue { get; private set; }

    private UserSubscription()
    {
    }

    public UserSubscription(Guid userId, Guid planId, string status, DateTime currentPeriodStart, DateTime currentPeriodEnd, bool autoRenew, string? billingIssue = null)
    {
        UserId = userId;
        PlanId = planId;
        Status = status;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        AutoRenew = autoRenew;
        BillingIssue = billingIssue;
    }

    public void UpdatePlan(Guid planId)
    {
        PlanId = planId;
        SetUpdated();
    }

    public void UpdateStatus(string status, string? billingIssue)
    {
        Status = status;
        BillingIssue = billingIssue;
        SetUpdated();
    }

    public void UpdateRenewal(bool autoRenew, DateTime currentPeriodEnd)
    {
        AutoRenew = autoRenew;
        CurrentPeriodEnd = currentPeriodEnd;
        SetUpdated();
    }
}

public static class UserSubscriptionStatus
{
    public const string Trial = "trial";
    public const string Active = "active";
    public const string PastDue = "past_due";
    public const string Canceled = "canceled";
}