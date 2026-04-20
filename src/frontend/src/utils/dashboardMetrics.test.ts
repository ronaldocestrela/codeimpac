import { describe, expect, it } from 'vitest'
import { buildDashboardMetrics } from './dashboardMetrics'

describe('buildDashboardMetrics', () => {
  it('uses paged contributions counters when both sources succeed', () => {
    const metrics = buildDashboardMetrics(
      {
        status: 'fulfilled',
        value: {
          items: [],
          totalCount: 42,
          commitCount: 30,
          pullRequestCount: 12,
          approvedPullRequestCount: 8,
          page: 1,
          pageSize: 20,
          totalPages: 3,
          hasPreviousPage: false,
          hasNextPage: true
        }
      },
      {
        status: 'fulfilled',
        value: [
          {
            id: 'r-1',
            generatedAt: '2026-04-20T12:00:00Z',
            commitCount: 20,
            pullRequestApprovedCount: 5,
            repositoryCount: 2,
            executiveSummaryPreview: 'preview'
          }
        ]
      }
    )

    expect(metrics.totalContributions).toBe(42)
    expect(metrics.commitCount).toBe(30)
    expect(metrics.pullRequestCount).toBe(12)
    expect(metrics.approvedPullRequestCount).toBe(8)
    expect(metrics.reportsCount).toBe(1)
    expect(metrics.latestReportAt).toBe('2026-04-20T12:00:00Z')
    expect(metrics.contributionsUnavailable).toBe(false)
    expect(metrics.reportsUnavailable).toBe(false)
  })

  it('keeps partial metrics when reports fail', () => {
    const metrics = buildDashboardMetrics(
      {
        status: 'fulfilled',
        value: {
          items: [],
          totalCount: 5,
          commitCount: 2,
          pullRequestCount: 3,
          approvedPullRequestCount: 1,
          page: 1,
          pageSize: 20,
          totalPages: 1,
          hasPreviousPage: false,
          hasNextPage: false
        }
      },
      {
        status: 'rejected',
        reason: new Error('network')
      }
    )

    expect(metrics.totalContributions).toBe(5)
    expect(metrics.reportsCount).toBe(0)
    expect(metrics.latestReportAt).toBeNull()
    expect(metrics.contributionsUnavailable).toBe(false)
    expect(metrics.reportsUnavailable).toBe(true)
  })
})
