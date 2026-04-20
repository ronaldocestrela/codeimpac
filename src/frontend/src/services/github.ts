import api from './api'
import axios from 'axios'
import type {
  GitHubAuthorizeUrlResponse,
  GitHubAccount,
  GitHubOrganization,
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

export const getLinkedGitHubAccount = async (): Promise<GitHubAccount | null> => {
  try {
    const response = await api.get<GitHubAccount>('github/account')
    if (response.status === 204) {
      return null
    }

    return response.data
  } catch (error) {
    if (axios.isAxiosError(error) && (error.response?.status === 204 || error.response?.status === 404)) {
      return null
    }

    throw error
  }
}

export const getGitHubRepositories = async (organizationLogin?: string): Promise<GitHubRepository[]> => {
  const response = await api.get<GitHubRepository[]>('github/repositories', {
    params: {
      organizationLogin
    }
  })
  return response.data
}

export const getGitHubOrganizations = async (): Promise<GitHubOrganization[]> => {
  const response = await api.get<GitHubOrganization[]>('github/organizations')
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
