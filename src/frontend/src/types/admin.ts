export interface AdminJobListItem {
  taskId: string
  userId: string
  jobType: string
  status: string
  createdAt: string
  startedAt?: string | null
  completedAt?: string | null
  errorMessage?: string | null
  hangfireJobId?: string | null
}

export interface AdminJobListResponse {
  items: AdminJobListItem[]
  totalCount: number
  page: number
  pageSize: number
}

export interface AdminUserListItem {
  userId: string
  email: string
  fullName: string
  accountStatus: string
  roles: string[]
  supportFlags: string[]
  createdAt: string
  lastLoginAt?: string | null
  subscriptionStatus?: string | null
  planName?: string | null
  repositoriesUsed: number
  reportsUsedThisMonth: number
}

export interface AdminUserListResponse {
  items: AdminUserListItem[]
  totalCount: number
  page: number
  pageSize: number
}

export interface AdminUserDetailResponse {
  user: AdminUserListItem
  lastSyncAt?: string | null
}

export interface AdminPlanOption {
  planId: string
  name: string
  description: string
  repositoriesLimit: number
  reportsPerMonth: number
  retentionDays: number
  isActive: boolean
}

export interface AdminUserSubscription {
  userId: string
  subscriptionId?: string | null
  planId?: string | null
  planName?: string | null
  status: string
  currentPeriodStart?: string | null
  currentPeriodEnd?: string | null
  autoRenew: boolean
  billingIssue?: string | null
  availablePlans: AdminPlanOption[]
}

export interface AdminAuditLogItem {
  id: string
  adminUserId: string
  action: string
  targetType: string
  targetId?: string | null
  payloadSummary: string
  result: string
  ipAddress?: string | null
  createdAt: string
}

export interface AdminAuditLogListResponse {
  items: AdminAuditLogItem[]
  totalCount: number
  page: number
  pageSize: number
}