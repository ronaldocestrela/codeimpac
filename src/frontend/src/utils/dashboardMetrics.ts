import type { PagedContributions } from '../types/contributions'
import type { ExecutiveReportListItem } from '../types/reports'

export interface DashboardMetrics {
  totalContributions: number
  commitCount: number
  pullRequestCount: number
  approvedPullRequestCount: number
  reportsCount: number
  latestReportAt: string | null
  contributionsUnavailable: boolean
  reportsUnavailable: boolean
}

export function buildDashboardMetrics(
  contributionsResult: PromiseSettledResult<PagedContributions>,
  reportsResult: PromiseSettledResult<ExecutiveReportListItem[]>
): DashboardMetrics {
  const contributions = contributionsResult.status === 'fulfilled'
    ? contributionsResult.value
    : null

  const reports = reportsResult.status === 'fulfilled'
    ? reportsResult.value
    : null

  return {
    totalContributions: contributions?.totalCount ?? 0,
    commitCount: contributions?.commitCount ?? 0,
    pullRequestCount: contributions?.pullRequestCount ?? 0,
    approvedPullRequestCount: contributions?.approvedPullRequestCount ?? 0,
    reportsCount: reports?.length ?? 0,
    latestReportAt: reports && reports.length > 0 ? reports[0].generatedAt : null,
    contributionsUnavailable: contributionsResult.status === 'rejected',
    reportsUnavailable: reportsResult.status === 'rejected'
  }
}
