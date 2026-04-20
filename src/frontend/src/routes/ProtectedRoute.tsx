import { Navigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'

interface ProtectedRouteProps {
  children: JSX.Element
  allowedRoles?: string[]
}

export default function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
  const accessToken = useAuthStore(state => state.accessToken)
  const hasAnyRole = useAuthStore(state => state.hasAnyRole)

  if (!accessToken) {
    return <Navigate to="/login" replace />
  }

  if (allowedRoles && allowedRoles.length > 0 && !hasAnyRole(allowedRoles)) {
    return <Navigate to="/dashboard" replace />
  }

  return children
}
