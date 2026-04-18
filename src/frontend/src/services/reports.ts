import api from './api'
import type { ExecutiveReport, ExecutiveReportFilters, ExecutiveReportListItem } from '../types/reports'
import type { BackgroundJobEnqueueResponse } from '../types/backgroundJobs'
import type { ReportExportFormat } from '../utils/reportExport'

export const enqueueExecutiveReportGeneration = async (filters: ExecutiveReportFilters = {}): Promise<BackgroundJobEnqueueResponse> => {
  const response = await api.post<BackgroundJobEnqueueResponse>('github/reports', {
    repositoryId: filters.repositoryId,
    from: filters.from,
    to: filters.to
  })

  return response.data
}

export const getExecutiveReports = async (filters: ExecutiveReportFilters = {}): Promise<ExecutiveReportListItem[]> => {
  const response = await api.get<ExecutiveReportListItem[]>('github/reports', {
    params: {
      repositoryId: filters.repositoryId,
      from: filters.from,
      to: filters.to
    }
  })

  return response.data
}

export const getExecutiveReportDetail = async (reportId: string): Promise<ExecutiveReport> => {
  const response = await api.get<ExecutiveReport>(`github/reports/${reportId}`)
  return response.data
}

export const exportExecutiveReport = async (
  reportId: string,
  format: ReportExportFormat
): Promise<{ blob: Blob, contentDisposition?: string }> => {
  const response = await api.get<Blob>(`github/reports/${reportId}/export`, {
    params: { format },
    responseType: 'blob'
  })

  const contentDisposition = response.headers['content-disposition'] as string | undefined
  return {
    blob: response.data,
    contentDisposition
  }
}
