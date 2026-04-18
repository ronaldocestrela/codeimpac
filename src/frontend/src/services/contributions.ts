import api from './api'
import type { ContributionDetail, ContributionsFilters, PagedContributions } from '../types/contributions'

export const getContributions = async (filters: ContributionsFilters = {}): Promise<PagedContributions> => {
  const response = await api.get<PagedContributions>('github/contributions', {
    params: {
      repositoryId: filters.repositoryId,
      from: filters.from,
      to: filters.to,
      page: filters.page,
      pageSize: filters.pageSize
    }
  })

  return response.data
}

export const getCommitContributionDetail = async (contributionId: string): Promise<ContributionDetail> => {
  const response = await api.get<ContributionDetail>(`github/contributions/commits/${contributionId}`)
  return response.data
}

export const getPullRequestContributionDetail = async (contributionId: string): Promise<ContributionDetail> => {
  const response = await api.get<ContributionDetail>(`github/contributions/pull-requests/${contributionId}`)
  return response.data
}
