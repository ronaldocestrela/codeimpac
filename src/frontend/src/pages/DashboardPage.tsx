import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { getGitHubAuthorizeUrl } from '../services/github'
import { getMe } from '../services/auth'

export default function DashboardPage() {
  const user = useAuthStore(state => state.user)
  const logout = useAuthStore(state => state.logout)
  const [message, setMessage] = useState('Carregando...')
  const navigate = useNavigate()

  useEffect(() => {
    if (!user) {
      getMe()
        .then(response => {
          if (response.email) {
            setMessage(`Bem-vindo, ${response.email}`)
          }
        })
        .catch(() => {
          logout()
          navigate('/login')
        })
    }
  }, [user, logout, navigate])

  const [loadingGitHub, setLoadingGitHub] = useState(false)

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const beginGitHubLink = async () => {
    setLoadingGitHub(true)
    try {
      const response = await getGitHubAuthorizeUrl()
      window.location.href = response.url
    } catch {
      setMessage('Falha ao iniciar o fluxo GitHub. Tente novamente.')
    } finally {
      setLoadingGitHub(false)
    }
  }

  return (
    <div className="max-w-3xl mx-auto p-6 bg-white rounded-lg shadow mt-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <button
          onClick={handleLogout}
          className="rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700"
        >
          Sair
        </button>
      </div>
      <p className="mt-4 text-gray-700">{message}</p>
      {user && (
        <div className="mt-6 rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600">Usuário autenticado:</p>
          <p className="mt-2 font-semibold">{user.email}</p>
          <p className="text-sm text-gray-500">ID: {user.subject}</p>
        </div>
      )}

      <div className="mt-6 rounded-lg border border-gray-200 p-4">
        <p className="text-sm text-gray-600">Integração GitHub</p>
        <button
          onClick={beginGitHubLink}
          disabled={loadingGitHub}
          className="mt-4 rounded-md bg-slate-800 px-4 py-2 text-white hover:bg-slate-900 disabled:opacity-60"
        >
          {loadingGitHub ? 'Redirecionando para o GitHub...' : 'Conectar GitHub'}
        </button>
      </div>
    </div>
  )
}
