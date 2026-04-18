namespace CodeImpact.Application.GitHub.Queries;

internal static class ContributionStatusMapper
{
    public static string BuildPullRequestStatus(bool isApproved, DateTime? mergedAtGitHub, string state)
    {
        if (isApproved)
        {
            return "approved";
        }

        if (mergedAtGitHub.HasValue)
        {
            return "merged";
        }

        if (string.Equals(state, "closed", StringComparison.OrdinalIgnoreCase))
        {
            return "closed";
        }

        return "open";
    }
}
