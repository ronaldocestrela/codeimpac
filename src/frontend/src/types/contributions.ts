export type ContributionType = 'commit' | 'pull_request'

export interface ContributionListItem {
  id: string
  type: ContributionType
  repositoryId: number
  repositoryFullName: string
  title: string
  author: string
  occurredAt: string
  status: string
  url: string
  isApproved?: boolean | null
}

export interface ContributionEvidence {
  evidenceType: string
  externalReference: string
  actor: string
  state: string
  occurredAt: string
  url: string
}

export interface ContributionDetail {
  id: string
  type: ContributionType
  repositoryId: number
  repositoryFullName: string
  externalReference: string
  title: string
  author: string
  occurredAt: string
  status: string
  url: string
  isApproved?: boolean | null
  evidence: ContributionEvidence[]
}

export interface ContributionsFilters {
  repositoryId?: number
  from?: string
  to?: string
}
