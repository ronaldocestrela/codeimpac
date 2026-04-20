export interface GitHubAccount {
  id: string
  gitHubUsername: string
  gitHubUserId: number
  linkedAt: string
}

export interface GitHubRepository {
  id: number
  name: string
  fullName: string
  private: boolean
  ownerLogin: string
  ownerType: string
  selected?: boolean
}

export interface GitHubOrganization {
  id: number
  login: string
  avatarUrl: string
}

export interface GitHubAuthorizeUrlResponse {
  url: string
}

export interface UpdateGitHubSelectionPayload {
  organizationLogin?: string
  repositories: Array<{
    id: number
    name: string
    fullName: string
    private: boolean
    ownerLogin: string
    ownerType: string
  }>
}
