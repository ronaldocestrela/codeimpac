export interface ExecutiveReportScope {
  developerScope: string
  repositoryId?: number | null
  from?: string | null
  to?: string | null
  repositories: string[]
}

export interface ExecutiveReportMetrics {
  commitCount: number
  pullRequestOpenCount: number
  pullRequestClosedCount: number
  pullRequestMergedCount: number
  pullRequestApprovedCount: number
  averageMergeLeadTimeHours?: number | null
  repositoryCount: number
}

export interface ExecutiveReportHighlight {
  title: string
  insight: string
  impact: string
  evidenceIds: string[]
}

export interface ExecutiveReportRisk {
  risk: string
  recommendation: string
  evidenceIds: string[]
}

export interface ExecutiveReportEvidence {
  evidenceId: string
  evidenceType: string
  repositoryFullName: string
  externalReference: string
  author: string
  occurredAt: string
  status: string
  url: string
}

export interface ExecutiveReport {
  id: string
  generatedAt: string
  scope: ExecutiveReportScope
  metrics: ExecutiveReportMetrics
  executiveSummary: string
  highlights: ExecutiveReportHighlight[]
  risks: ExecutiveReportRisk[]
  evidence: ExecutiveReportEvidence[]
}

export interface ExecutiveReportListItem {
  id: string
  generatedAt: string
  repositoryId?: number | null
  from?: string | null
  to?: string | null
  commitCount: number
  pullRequestApprovedCount: number
  repositoryCount: number
  executiveSummaryPreview: string
}

export interface ExecutiveReportFilters {
  repositoryId?: number
  from?: string
  to?: string
}
