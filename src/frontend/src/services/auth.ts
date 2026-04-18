import axios from 'axios'
import api from './api'
import type { AuthResult, LoginPayload, RefreshRequest, UserInfo } from '../types/auth'

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5262/api'
const refreshClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json'
  }
})

export const login = async (payload: LoginPayload): Promise<AuthResult> => {
  const response = await api.post<AuthResult>('auth/login', payload)
  return response.data
}

export const register = async (payload: LoginPayload): Promise<AuthResult> => {
  const response = await api.post<AuthResult>('auth/register', payload)
  return response.data
}

export const refreshToken = async (payload: RefreshRequest): Promise<AuthResult> => {
  const response = await refreshClient.post<AuthResult>('auth/refresh', payload)
  return response.data
}

export const getMe = async (): Promise<UserInfo> => {
  const response = await api.get<UserInfo>('auth/me')
  return response.data
}
