import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '../store/authStore'
import { getGitHubAuthorizeUrl, getLinkedGitHubAccount, getSelectedGitHubRepositories } from '../services/github'
import { getMe } from '../services/auth'
import { getContributions } from '../services/contributions'
import { getExecutiveReports } from '../services/reports'
import type { GitHubRepository } from '../types/github'

function toDateInputValue(date: Date): string {
  return date.toISOString().split('T')[0]
}

function formatNumber(value: number): string {
  return new Intl.NumberFormat('pt-BR').format(value)
}

export default function DashboardPage() {
  const user = useAuthStore(state => state.user)
  const setUser = useAuthStore(state => state.setUser)
  const logout = useAuthStore(state => state.logout)
  const [message, setMessage] = useState('Carregando...')
  const [loadingProfile, setLoadingProfile] = useState(true)
  const [loadingGitHub, setLoadingGitHub] = useState(false)
  const [to, setTo] = useState(() => toDateInputValue(new Date()))
  const [from, setFrom] = useState(() => {
    const fromDate = new Date()
    fromDate.setDate(fromDate.getDate() - 30)
    return toDateInputValue(fromDate)
  })
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

  const filters = useMemo(() => ({
    from: from || undefined,
    to: to || undefined
  }), [from, to])

  const githubStatusQuery = useQuery({
    queryKey: ['dashboard-github-status'],
    queryFn: async (): Promise<{ githubLinked: boolean, selectedRepositories: GitHubRepository[] }> => {
      const account = await getLinkedGitHubAccount()
      if (!account) {
        return { githubLinked: false, selectedRepositories: [] }
      }

      const selectedRepositories = await getSelectedGitHubRepositories()
      return { githubLinked: true, selectedRepositories }
    }
  })

  const metricsQuery = useQuery({
    queryKey: ['dashboard-metrics', filters],
    queryFn: async () => {
      const [contributions, reports] = await Promise.all([
        getContributions(filters),
        getExecutiveReports(filters)
      ])

      const pullRequests = contributions.filter(item => item.type === 'pull_request')
      const commits = contributions.filter(item => item.type === 'commit')
      const approvedPullRequests = pullRequests.filter(item => item.isApproved === true)

      return {
        totalContributions: contributions.length,
        commitCount: commits.length,
        pullRequestCount: pullRequests.length,
        approvedPullRequestCount: approvedPullRequests.length,
        reportsCount: reports.length,
        latestReportAt: reports.length > 0 ? reports[0].generatedAt : null
      }
    },
    retry: false
  })

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

  const githubLinked = githubStatusQuery.data?.githubLinked ?? false
  const selectedRepositories = githubStatusQuery.data?.selectedRepositories ?? []

  const clearRange = () => {
    setFrom('')
    setTo('')
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
        <div className="flex flex-wrap items-center justify-between gap-3">
          <p className="text-sm text-gray-600">Escopo do Dashboard</p>
          <button
            type="button"
            onClick={clearRange}
            className="text-sm text-indigo-700 hover:underline"
          >
            Limpar período
          </button>
        </div>

        <div className="mt-3 grid gap-3 md:grid-cols-2">
          <label className="text-sm text-slate-700">
            De
            <input
              className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
              type="date"
              value={from}
              onChange={event => setFrom(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-700">
            Até
            <input
              className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
              type="date"
              value={to}
              onChange={event => setTo(event.target.value)}
            />
          </label>
        </div>

        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">Contribuições</p>
            <p className="mt-1 text-xl font-semibold text-slate-900">{formatNumber(metricsQuery.data?.totalContributions ?? 0)}</p>
          </div>
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">Commits</p>
            <p className="mt-1 text-xl font-semibold text-slate-900">{formatNumber(metricsQuery.data?.commitCount ?? 0)}</p>
          </div>
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">PRs aprovados</p>
            <p className="mt-1 text-xl font-semibold text-slate-900">{formatNumber(metricsQuery.data?.approvedPullRequestCount ?? 0)}</p>
          </div>
        </div>

        <div className="mt-3 grid gap-3 md:grid-cols-2">
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">Relatórios gerados</p>
            <p className="mt-1 text-xl font-semibold text-slate-900">{formatNumber(metricsQuery.data?.reportsCount ?? 0)}</p>
          </div>
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">Último relatório</p>
            <p className="mt-1 text-sm font-semibold text-slate-900">{metricsQuery.data?.latestReportAt ? new Date(metricsQuery.data.latestReportAt).toLocaleString() : 'Sem relatórios'}</p>
          </div>
        </div>

        {metricsQuery.isLoading && (
          <p className="mt-3 text-sm text-slate-500">Calculando métricas do período...</p>
        )}
        {metricsQuery.isError && (
          <p className="mt-3 text-sm text-amber-700">Não foi possível carregar métricas agregadas com o período informado.</p>
        )}
      </div>

      <div className="mt-6 rounded-lg border border-gray-200 p-4">
        <p className="text-sm text-gray-600">Integração GitHub</p>

        {githubStatusQuery.isLoading && (
          <p className="mt-2 text-sm text-gray-500">Verificando status da conta GitHub...</p>
        )}

        {!githubStatusQuery.isLoading && githubLinked && (
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

        {!githubStatusQuery.isLoading && !githubLinked && (
          <button
            onClick={beginGitHubLink}
            disabled={loadingGitHub}
            className="mt-4 rounded-md bg-slate-800 px-4 py-2 text-white hover:bg-slate-900 disabled:opacity-60"
          >
            {loadingGitHub ? 'Redirecionando para o GitHub...' : 'Conectar GitHub'}
          </button>
        )}

        {githubStatusQuery.isError && (
          <p className="mt-3 text-sm text-amber-700">Não foi possível validar a integração GitHub no momento.</p>
        )}
      </div>

      <div className="mt-6 rounded-lg border border-gray-200 p-4">
        <p className="text-sm text-gray-600">Ações rápidas</p>
        <div className="mt-3 flex flex-wrap gap-2">
          <Link className="rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-700 hover:bg-slate-50" to="/contributions">
            Ver Contribuições
          </Link>
          <Link className="rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-700 hover:bg-slate-50" to="/reports">
            Ver Relatórios
          </Link>
          <Link className="rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-700 hover:bg-slate-50" to="/github/repositories">
            Ajustar Repositórios
          </Link>
        </div>
      </div>
    </div>
  )
}
