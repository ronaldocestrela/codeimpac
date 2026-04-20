import { persist } from 'zustand/middleware'
import { create } from 'zustand'
import type { AuthResult, UserInfo } from '../types/auth'
import type { GitHubAccount } from '../types/github'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  user: UserInfo | null
  githubAccount: GitHubAccount | null
  setAuthTokens: (auth: AuthResult) => void
  setUser: (user: UserInfo) => void
  setGitHubAccount: (account: GitHubAccount | null) => void
  logout: () => void
  hasAnyRole: (roles: string[]) => boolean
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      githubAccount: null,
      setAuthTokens: auth => set({ accessToken: auth.accessToken, refreshToken: auth.refreshToken }),
      setUser: user => set({ user }),
      setGitHubAccount: account => set({ githubAccount: account }),
      logout: () => set({ accessToken: null, refreshToken: null, user: null, githubAccount: null }),
      hasAnyRole: roles => {
        const userRoles = get().user?.roles ?? []
        return roles.some(role => userRoles.includes(role))
      }
    }),
    {
      name: 'codeimpact-auth',
      partialize: state => ({ refreshToken: state.refreshToken })
    }
  )
)

export const getAuthState = () => useAuthStore.getState()
