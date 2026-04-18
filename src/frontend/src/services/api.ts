import axios from 'axios'
import { getAuthState, useAuthStore } from '../store/authStore'
import type { AuthResult } from '../types/auth'

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5262/api'

const api = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json'
  }
})

const refreshClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json'
  }
})

let refreshingPromise: Promise<AuthResult> | null = null

api.interceptors.request.use(config => {
  const accessToken = getAuthState().accessToken

  if (accessToken && config.headers) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }

  return config
})

api.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config as any

    if (error.response?.status === 401 && !originalRequest._retry) {
      const refreshToken = getAuthState().refreshToken
      if (!refreshToken) {
        useAuthStore.getState().logout()
        return Promise.reject(error)
      }

      originalRequest._retry = true

      try {
        if (!refreshingPromise) {
          refreshingPromise = refreshClient
            .post<AuthResult>('auth/refresh', { refreshToken })
            .then(response => response.data)
        }

        const auth = await refreshingPromise
        refreshingPromise = null

        useAuthStore.getState().setAuthTokens(auth)
        originalRequest.headers.Authorization = `${auth.tokenType} ${auth.accessToken}`

        return api(originalRequest)
      } catch (refreshError) {
        useAuthStore.getState().logout()
        refreshingPromise = null
        return Promise.reject(refreshError)
      }
    }

    return Promise.reject(error)
  }
)

export default api
