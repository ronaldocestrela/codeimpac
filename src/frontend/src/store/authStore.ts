import { persist } from 'zustand/middleware'
import { create } from 'zustand'
import type { AuthResult, UserInfo } from '../types/auth'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  user: UserInfo | null
  setAuthTokens: (auth: AuthResult) => void
  setUser: (user: UserInfo) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    set => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      setAuthTokens: auth => set({ accessToken: auth.accessToken, refreshToken: auth.refreshToken }),
      setUser: user => set({ user }),
      logout: () => set({ accessToken: null, refreshToken: null, user: null })
    }),
    {
      name: 'codeimpact-auth',
      partialize: state => ({ refreshToken: state.refreshToken })
    }
  )
)

export const getAuthState = () => useAuthStore.getState()
