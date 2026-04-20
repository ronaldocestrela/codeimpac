import { useEffect, useState } from 'react'
import { Routes, Route, Link } from 'react-router-dom'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import DashboardPage from './pages/DashboardPage'
import GitHubOAuthCallbackPage from './pages/GitHubOAuthCallbackPage'
import RepositorySelectionPage from './pages/RepositorySelectionPage'
import ContributionsPage from './pages/ContributionsPage'
import ContributionDetailPage from './pages/ContributionDetailPage'
import ReportsPage from './pages/ReportsPage'
import AdminUsersPage from './pages/AdminUsersPage'
import AdminUserDetailPage from './pages/AdminUserDetailPage'
import AdminJobsPage from './pages/AdminJobsPage'
import AdminAuditPage from './pages/AdminAuditPage'
import AdminSubscriptionsPage from './pages/AdminSubscriptionsPage'
import ProtectedRoute from './routes/ProtectedRoute'
import { useAuthStore } from './store/authStore'
import { getMe, refreshToken as refreshSession } from './services/auth'

function Home() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center text-center px-4">
      <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant mb-4">Developer Intelligence Platform</p>
      <h1 className="text-5xl font-semibold text-on-surface leading-tight">
        Code<span className="text-primary">Impact</span>
      </h1>
      <p className="mt-4 max-w-md text-sm text-on-surface-variant leading-relaxed">
        Analise contribuições, classifique pull requests aprovados e gere relatórios executivos rastreáveis para liderança.
      </p>
      <div className="mt-8 flex gap-3">
        <Link to="/login" className="btn-primary">Entrar</Link>
        <Link to="/register" className="btn-secondary">Criar conta</Link>
      </div>
    </div>
  )
}

export default function App() {
  const accessToken = useAuthStore(state => state.accessToken)
  const refreshToken = useAuthStore(state => state.refreshToken)
  const setAuthTokens = useAuthStore(state => state.setAuthTokens)
  const setUser = useAuthStore(state => state.setUser)
  const logout = useAuthStore(state => state.logout)
  const user = useAuthStore(state => state.user)
  const [initializing, setInitializing] = useState(true)
  const isAdmin = (user?.roles ?? []).some(role => role === 'Owner' || role === 'Admin')

  useEffect(() => {
    if (!accessToken && refreshToken) {
      refreshSession({ refreshToken })
        .then(auth => {
          setAuthTokens(auth)
          return getMe()
        })
        .then(setUser)
        .catch(() => logout())
        .finally(() => setInitializing(false))
    } else {
      setInitializing(false)
    }
  }, [accessToken, refreshToken, setAuthTokens, setUser, logout])

  if (initializing) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-surface">
        <p className="text-sm text-on-surface-variant">Carregando sessão...</p>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-surface">
      <header className="bg-surface-container-low px-6 py-4">
        <div className="mx-auto flex max-w-6xl items-center justify-between">
          <Link to="/" className="text-lg font-semibold tracking-tight text-on-surface">
            Code<span className="text-primary">Impact</span>
          </Link>
          <nav className="flex items-center gap-6 text-sm">
            {accessToken ? (
              <>
                <Link to="/dashboard" className="text-on-surface-variant hover:text-on-surface transition-colors">Dashboard</Link>
                <Link to="/contributions" className="text-on-surface-variant hover:text-on-surface transition-colors">Contribuições</Link>
                <Link to="/reports" className="text-on-surface-variant hover:text-on-surface transition-colors">Relatórios</Link>
                {isAdmin && <Link to="/admin/users" className="text-on-surface-variant hover:text-on-surface transition-colors">Backoffice</Link>}
              </>
            ) : (
              <>
                <Link to="/login" className="text-on-surface-variant hover:text-on-surface transition-colors">Login</Link>
                <Link to="/register" className="btn-secondary text-xs py-1.5">Criar conta</Link>
              </>
            )}
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-6 py-8">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/dashboard" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
          <Route path="/github/callback" element={<ProtectedRoute><GitHubOAuthCallbackPage /></ProtectedRoute>} />
          <Route path="/github/repositories" element={<ProtectedRoute><RepositorySelectionPage /></ProtectedRoute>} />
          <Route path="/contributions" element={<ProtectedRoute><ContributionsPage /></ProtectedRoute>} />
          <Route path="/contributions/:type/:id" element={<ProtectedRoute><ContributionDetailPage /></ProtectedRoute>} />
          <Route path="/reports" element={<ProtectedRoute><ReportsPage /></ProtectedRoute>} />
          <Route path="/admin/users" element={<ProtectedRoute allowedRoles={['Owner', 'Admin']}><AdminUsersPage /></ProtectedRoute>} />
          <Route path="/admin/users/:id" element={<ProtectedRoute allowedRoles={['Owner', 'Admin']}><AdminUserDetailPage /></ProtectedRoute>} />
          <Route path="/admin/jobs" element={<ProtectedRoute allowedRoles={['Owner', 'Admin']}><AdminJobsPage /></ProtectedRoute>} />
          <Route path="/admin/subscriptions" element={<ProtectedRoute allowedRoles={['Owner', 'Admin']}><AdminSubscriptionsPage /></ProtectedRoute>} />
          <Route path="/admin/audit" element={<ProtectedRoute allowedRoles={['Owner', 'Admin']}><AdminAuditPage /></ProtectedRoute>} />
        </Routes>
      </main>
    </div>
  )
}
