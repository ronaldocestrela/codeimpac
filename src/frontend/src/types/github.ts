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
  selected?: boolean
}

export interface GitHubAuthorizeUrlResponse {
  url: string
}

export interface UpdateGitHubSelectionPayload {
  repositories: Array<{
    id: number
    name: string
    fullName: string
    private: boolean
  }>
}
