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
import ProtectedRoute from './routes/ProtectedRoute'
import { useAuthStore } from './store/authStore'
import { getMe, refreshToken as refreshSession } from './services/auth'

function Home() {
  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold">CodeImpact Frontend</h1>
      <p className="mt-2">Bem-vindo ao frontend do CodeImpact.</p>
      <div className="mt-4 space-x-4">
        <Link to="/login" className="text-indigo-600">Login</Link>
        <Link to="/register" className="text-indigo-600">Cadastrar</Link>
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
  const [initializing, setInitializing] = useState(true)

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
    return <div className="p-4">Carregando sessão...</div>
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b bg-white px-6 py-4 shadow-sm">
        <div className="mx-auto flex max-w-6xl items-center justify-between">
          <div>
            <Link to="/" className="text-xl font-bold text-slate-900">CodeImpact</Link>
          </div>
          <nav className="space-x-4 text-slate-700">
            <Link to="/login">Login</Link>
            <Link to="/register">Cadastro</Link>
            <Link to="/dashboard">Dashboard</Link>
            <Link to="/contributions">Contribuições</Link>
            <Link to="/reports">Relatórios</Link>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-6xl p-6">
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
        </Routes>
      </main>
    </div>
  )
}
