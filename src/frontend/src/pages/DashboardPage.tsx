import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { getGitHubAuthorizeUrl, getLinkedGitHubAccount, getSelectedGitHubRepositories } from '../services/github'
import { getMe } from '../services/auth'
import type { GitHubRepository } from '../types/github'

export default function DashboardPage() {
  const user = useAuthStore(state => state.user)
  const setUser = useAuthStore(state => state.setUser)
  const logout = useAuthStore(state => state.logout)
  const [message, setMessage] = useState('Carregando...')
  const [loadingProfile, setLoadingProfile] = useState(true)
  const [loadingGitHubStatus, setLoadingGitHubStatus] = useState(true)
  const [githubLinked, setGithubLinked] = useState(false)
  const [selectedRepositories, setSelectedRepositories] = useState<GitHubRepository[]>([])
  const [loadingGitHub, setLoadingGitHub] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    const hydrateDashboard = async () => {
      try {
        const resolvedUser = user ?? await getMe()
        if (!user) {
          setUser(resolvedUser)
        }

        setMessage(resolvedUser.email ? `Bem-vindo, ${resolvedUser.email}` : 'Sessão carregada com sucesso.')
      } catch {
        logout()
        navigate('/login')
      } finally {
        setLoadingProfile(false)
      }
    }

    void hydrateDashboard()
  }, [user, setUser, logout, navigate])

  useEffect(() => {
    const loadGitHubStatus = async () => {
      setLoadingGitHubStatus(true)
      try {
        const account = await getLinkedGitHubAccount()
        setGithubLinked(Boolean(account))

        if (!account) {
          setSelectedRepositories([])
          return
        }

        const repositories = await getSelectedGitHubRepositories()
        setSelectedRepositories(repositories)
      } catch {
        setGithubLinked(false)
        setSelectedRepositories([])
      } finally {
        setLoadingGitHubStatus(false)
      }
    }

    void loadGitHubStatus()
  }, [])

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

  const goToRepositorySelection = () => {
    navigate('/github/repositories')
  }

  if (loadingProfile) {
    return (
      <div className="max-w-3xl mx-auto p-6 bg-white rounded-lg shadow mt-8">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <p className="mt-4 text-gray-700">Carregando perfil...</p>
      </div>
    )
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

        {loadingGitHubStatus && (
          <p className="mt-2 text-sm text-gray-500">Verificando status da conta GitHub...</p>
        )}

        {!loadingGitHubStatus && githubLinked && (
          <div className="mt-3 space-y-3">
            <p className="text-sm text-green-700">Conta GitHub vinculada com sucesso.</p>
            <p className="text-sm text-gray-600">
              Repositórios selecionados: <strong>{selectedRepositories.length}</strong>
            </p>

            {selectedRepositories.length > 0 && (
              <ul className="list-disc pl-5 text-sm text-gray-700">
                {selectedRepositories.slice(0, 5).map(repo => (
                  <li key={repo.id}>{repo.fullName}</li>
                ))}
              </ul>
            )}

            <button
              onClick={goToRepositorySelection}
              className="rounded-md bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-700"
            >
              Gerenciar Repositórios
            </button>
          </div>
        )}

        {!loadingGitHubStatus && !githubLinked && (
          <button
            onClick={beginGitHubLink}
            disabled={loadingGitHub}
            className="mt-4 rounded-md bg-slate-800 px-4 py-2 text-white hover:bg-slate-900 disabled:opacity-60"
          >
            {loadingGitHub ? 'Redirecionando para o GitHub...' : 'Conectar GitHub'}
          </button>
        )}
      </div>
    </div>
  )
}
