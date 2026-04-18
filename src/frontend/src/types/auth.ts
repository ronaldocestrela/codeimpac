export interface LoginPayload {
  email: string
  password: string
}

export interface AuthResult {
  accessToken: string
  refreshToken: string
  tokenType: string
}

export interface UserInfo {
  email: string
  subject: string
}

export interface RefreshRequest {
  refreshToken: string
}
