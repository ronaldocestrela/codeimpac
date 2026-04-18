import { Navigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'

interface ProtectedRouteProps {
  children: JSX.Element
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const accessToken = useAuthStore(state => state.accessToken)

  if (!accessToken) {
    return <Navigate to="/login" replace />
  }

  return children
}
