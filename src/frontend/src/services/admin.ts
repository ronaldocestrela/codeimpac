import api from './api'
import type {
  AdminAuditLogListResponse,
  AdminJobListItem,
  AdminJobListResponse,
  AdminUserDetailResponse,
  AdminUserListResponse,
  AdminUserSubscription
} from '../types/admin'

export const getAdminJobs = async (params: {
  jobType?: string
  status?: string
  page?: number
  pageSize?: number
}): Promise<AdminJobListResponse> => {
  const response = await api.get<AdminJobListResponse>('admin/jobs', { params })
  return response.data
}

export const getAdminJobDetail = async (taskId: string): Promise<AdminJobListItem> => {
  const response = await api.get<AdminJobListItem>(`admin/jobs/${taskId}`)
  return response.data
}

export const retryAdminJob = async (taskId: string): Promise<{ taskId: string }> => {
  const response = await api.post<{ taskId: string }>(`admin/jobs/${taskId}/retry`)
  return response.data
}

export const getAdminUsers = async (params: {
  email?: string
  status?: string
  page?: number
  pageSize?: number
}): Promise<AdminUserListResponse> => {
  const response = await api.get<AdminUserListResponse>('admin/users', { params })
  return response.data
}

export const getAdminUserDetail = async (userId: string): Promise<AdminUserDetailResponse> => {
  const response = await api.get<AdminUserDetailResponse>(`admin/users/${userId}`)
  return response.data
}

export const updateAdminUserStatus = async (userId: string, status: string, reason?: string): Promise<void> => {
  await api.patch(`admin/users/${userId}/status`, { status, reason })
}

export const updateAdminUserSupportFlags = async (userId: string, supportFlags: string[]): Promise<void> => {
  await api.patch(`admin/users/${userId}/support-flags`, { supportFlags })
}

export const revokeAdminUserGitHubAccess = async (userId: string): Promise<void> => {
  await api.post(`admin/users/${userId}/revoke-github`)
}

export const forceAdminUserResync = async (userId: string): Promise<{ syncedRepositories: number }> => {
  const response = await api.post<{ syncedRepositories: number }>(`admin/users/${userId}/force-resync`)
  return response.data
}

export const getAdminUserSubscription = async (userId: string): Promise<AdminUserSubscription> => {
  const response = await api.get<AdminUserSubscription>(`admin/users/${userId}/subscription`)
  return response.data
}

export const updateAdminUserSubscription = async (
  userId: string,
  payload: {
    planId: string
    status: string
    autoRenew: boolean
    currentPeriodEnd: string
    billingIssue?: string
  }
): Promise<void> => {
  await api.patch(`admin/users/${userId}/subscription`, payload)
}

export const getAdminAuditLogs = async (params: {
  action?: string
  targetType?: string
  adminUserId?: string
  page?: number
  pageSize?: number
}): Promise<AdminAuditLogListResponse> => {
  const response = await api.get<AdminAuditLogListResponse>('admin/audit-logs', { params })
  return response.data
}