import api from './api'
import type { ExecutiveReport, ExecutiveReportFilters, ExecutiveReportListItem } from '../types/reports'

export const generateExecutiveReport = async (filters: ExecutiveReportFilters = {}): Promise<ExecutiveReport> => {
  const response = await api.post<ExecutiveReport>('github/reports', {
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
