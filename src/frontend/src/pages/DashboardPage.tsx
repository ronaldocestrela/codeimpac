import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '../store/authStore'
import { getGitHubAuthorizeUrl, getLinkedGitHubAccount, getSelectedGitHubRepositories } from '../services/github'
import { getMe } from '../services/auth'
import { getContributions } from '../services/contributions'
import { getExecutiveReports } from '../services/reports'
import type { GitHubRepository } from '../types/github'
import { buildDashboardMetrics } from '../utils/dashboardMetrics'

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

  const hasInvalidRange = Boolean(from && to && from > to)

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
      const [contributionsResult, reportsResult] = await Promise.allSettled([
        getContributions(filters),
        getExecutiveReports(filters)
      ])

      return buildDashboardMetrics(contributionsResult, reportsResult)
    },
    retry: false,
    enabled: !hasInvalidRange
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
      <div className="flex min-h-[50vh] items-center justify-center">
        <p className="text-sm text-on-surface-variant">Carregando perfil...</p>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Visão geral</p>
          <h1 className="mt-1 text-3xl font-semibold text-on-surface">Dashboard</h1>
          {message && <p className="mt-1 text-sm text-on-surface-variant">{message}</p>}
        </div>
        <button
          onClick={handleLogout}
          className="btn-secondary text-xs"
        >
          Sair
        </button>
      </div>

      {/* User info */}
      {user && (
        <div className="card flex items-center justify-between gap-4">
          <div>
            <p className="text-xs text-on-surface-variant field-label">Usuário autenticado</p>
            <p className="mt-1 font-semibold text-on-surface">{user.email}</p>
          </div>
          <p className="text-xs text-on-surface-variant font-mono">{user.subject}</p>
        </div>
      )}

      {/* Date scope */}
      <div className="card">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Período analisado</p>
          <button type="button" onClick={clearRange} className="btn-ghost text-xs py-0">
            Limpar período
          </button>
        </div>

        <div className="mt-3 grid gap-3 md:grid-cols-2">
          <label className="text-xs text-on-surface-variant">
            De
            <input
              className="mt-1.5 w-full px-3 py-2"
              type="date"
              value={from}
              onChange={event => setFrom(event.target.value)}
            />
          </label>
          <label className="text-xs text-on-surface-variant">
            Até
            <input
              className="mt-1.5 w-full px-3 py-2"
              type="date"
              value={to}
              onChange={event => setTo(event.target.value)}
            />
          </label>
        </div>

        {/* Metrics */}
        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">Contribuições</p>
            <p className="mt-1 text-2xl font-semibold text-on-surface">{formatNumber(metricsQuery.data?.totalContributions ?? 0)}</p>
          </div>
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">Commits</p>
            <p className="mt-1 text-2xl font-semibold text-on-surface">{formatNumber(metricsQuery.data?.commitCount ?? 0)}</p>
          </div>
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">PRs aprovados</p>
            <p className="mt-1 text-2xl font-semibold text-primary">{formatNumber(metricsQuery.data?.approvedPullRequestCount ?? 0)}</p>
          </div>
        </div>

        <div className="mt-3 grid gap-3 md:grid-cols-2">
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">Relatórios gerados</p>
            <p className="mt-1 text-2xl font-semibold text-on-surface">{formatNumber(metricsQuery.data?.reportsCount ?? 0)}</p>
          </div>
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">Último relatório</p>
            <p className="mt-1 text-sm font-semibold text-on-surface">
              {metricsQuery.data?.latestReportAt
                ? new Date(metricsQuery.data.latestReportAt).toLocaleString()
                : <span className="text-on-surface-variant">Sem relatórios</span>
              }
            </p>
          </div>
        </div>

        {metricsQuery.isLoading && (
          <p className="mt-3 text-xs text-on-surface-variant">Calculando métricas do período...</p>
        )}
        {hasInvalidRange && (
          <p className="mt-3 text-xs text-tertiary">Período inválido: a data inicial deve ser menor ou igual à data final.</p>
        )}
        {!hasInvalidRange && metricsQuery.data?.contributionsUnavailable && (
          <p className="mt-3 text-xs text-tertiary">Não foi possível carregar contribuições para o período informado.</p>
        )}
        {!hasInvalidRange && metricsQuery.data?.reportsUnavailable && (
          <p className="mt-1 text-xs text-tertiary">Não foi possível carregar o histórico de relatórios para o período informado.</p>
        )}
        {metricsQuery.isError && (
          <div className="mt-3 flex flex-wrap items-center gap-2">
            <p className="text-xs text-tertiary">Não foi possível carregar métricas para o período informado.</p>
            <button type="button" className="btn-ghost text-xs py-0" onClick={() => void metricsQuery.refetch()}>
              Tentar novamente
            </button>
          </div>
        )}
      </div>

      {/* GitHub integration */}
      <div className="card">
        <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Integração GitHub</p>

        {githubStatusQuery.isLoading && (
          <p className="mt-3 text-sm text-on-surface-variant">Verificando status da conta GitHub...</p>
        )}

        {!githubStatusQuery.isLoading && githubLinked && (
          <div className="mt-4 space-y-3">
            <p className="text-sm text-success">Conta GitHub vinculada com sucesso.</p>
            <p className="text-sm text-on-surface-variant">
              Repositórios selecionados:{' '}
              <span className="font-semibold text-on-surface">{selectedRepositories.length}</span>
            </p>

            {selectedRepositories.length > 0 && (
              <ul className="space-y-1">
                {selectedRepositories.slice(0, 5).map(repo => (
                  <li key={repo.id} className="text-xs text-on-surface-variant font-mono bg-surface-container-lowest px-3 py-1.5 rounded-sm">
                    {repo.fullName}
                  </li>
                ))}
              </ul>
            )}

            <div className="flex flex-wrap gap-2">
              <button onClick={goToRepositorySelection} className="btn-secondary text-xs">
                Gerenciar Repositórios
              </button>
              <button onClick={beginGitHubLink} disabled={loadingGitHub} className="btn-secondary text-xs">
                {loadingGitHub ? 'Redirecionando...' : 'Solicitar acesso a outras organizações'}
              </button>
            </div>
          </div>
        )}

        {!githubStatusQuery.isLoading && !githubLinked && (
          <button
            onClick={beginGitHubLink}
            disabled={loadingGitHub}
            className="mt-4 btn-primary"
          >
            {loadingGitHub ? 'Redirecionando...' : 'Conectar GitHub'}
          </button>
        )}

        {githubStatusQuery.isError && (
          <p className="mt-3 text-xs text-tertiary">Não foi possível validar a integração GitHub no momento.</p>
        )}
      </div>

      {/* Quick actions */}
      <div className="card">
        <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Ações rápidas</p>
        <div className="mt-3 flex flex-wrap gap-2">
          <Link className="btn-secondary text-xs" to="/contributions">Ver Contribuições</Link>
          <Link className="btn-secondary text-xs" to="/reports">Ver Relatórios</Link>
          <Link className="btn-secondary text-xs" to="/github/repositories">Ajustar Repositórios</Link>
        </div>
      </div>
    </div>
  )
}
