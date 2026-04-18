export type BackgroundJobType = 'ContributionSummary' | 'ExecutiveReport'
export type BackgroundJobStatus = 'Queued' | 'Processing' | 'Succeeded' | 'Failed'

export interface BackgroundJobEnqueueResponse {
  taskId: string
  jobType: BackgroundJobType
  status: BackgroundJobStatus
  hangfireJobId?: string | null
  createdAt: string
}

export interface BackgroundJobStatusResponse {
  taskId: string
  jobType: BackgroundJobType
  status: BackgroundJobStatus
  createdAt: string
  startedAt?: string | null
  completedAt?: string | null
  errorMessage?: string | null
  hangfireJobId?: string | null
  reportId?: string | null
  contributionSummary?: unknown
}
