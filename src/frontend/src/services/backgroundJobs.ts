import api from './api'
import type { BackgroundJobStatusResponse } from '../types/backgroundJobs'

export const getBackgroundJobStatus = async (taskId: string): Promise<BackgroundJobStatusResponse> => {
  const response = await api.get<BackgroundJobStatusResponse>(`github/jobs/${taskId}`)
  return response.data
}
