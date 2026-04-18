import api from './api'
import type {
  GitHubAuthorizeUrlResponse,
  GitHubAccount,
  GitHubRepository,
  UpdateGitHubSelectionPayload
} from '../types/github'

export const getGitHubAuthorizeUrl = async (): Promise<GitHubAuthorizeUrlResponse> => {
  const response = await api.get<GitHubAuthorizeUrlResponse>('github/authorize-url')
  return response.data
}

export const linkGitHubAccount = async (code: string): Promise<GitHubAccount> => {
  const response = await api.post<GitHubAccount>('github/callback', { code })
  return response.data
}

export const getGitHubRepositories = async (): Promise<GitHubRepository[]> => {
  const response = await api.get<GitHubRepository[]>('github/repositories')
  return response.data
}

export const getSelectedGitHubRepositories = async (): Promise<GitHubRepository[]> => {
  const response = await api.get<GitHubRepository[]>('github/repositories/selected')
  return response.data
}

export const updateSelectedGitHubRepositories = async (payload: UpdateGitHubSelectionPayload): Promise<void> => {
  await api.post('github/repositories/select', payload)
}

export const syncGitHubRepository = async (repositoryId: number): Promise<void> => {
  await api.post(`github/repositories/${repositoryId}/sync`)
}
