import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getContributions } from '../services/contributions'
import { getGitHubRepositories, getSelectedGitHubRepositories } from '../services/github'
import type { ContributionListItem } from '../types/contributions'
import type { GitHubRepository } from '../types/github'

function formatNumber(value: number): string {
  return new Intl.NumberFormat('pt-BR').format(value)
}

export default function ContributionsPage() {
  const [repositoryId, setRepositoryId] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)

  const filters = useMemo(() => ({
    repositoryId: repositoryId ? Number(repositoryId) : undefined,
    from: from || undefined,
    to: to || undefined,
    page,
    pageSize
  }), [repositoryId, from, to, page, pageSize])

  useEffect(() => {
    setPage(1)
  }, [repositoryId, from, to])

  const repositoriesQuery = useQuery({
    queryKey: ['contributions-repositories'],
    queryFn: async (): Promise<GitHubRepository[]> => {
      const selected = await getSelectedGitHubRepositories()
      if (selected.length > 0) {
        return selected
      }

      return getGitHubRepositories()
    }
  })

  const contributionsQuery = useQuery({
    queryKey: ['contributions', filters],
    queryFn: () => getContributions(filters)
  })

  const contributionsPage = contributionsQuery.data
  const contributions = contributionsPage?.items ?? []
  const repositories = repositoriesQuery.data ?? []

  const metrics = useMemo(() => {
    return {
      total: contributionsPage?.totalCount ?? 0,
      commits: contributionsPage?.commitCount ?? 0,
      pullRequests: contributionsPage?.pullRequestCount ?? 0,
      approvedPullRequests: contributionsPage?.approvedPullRequestCount ?? 0
    }
  }, [contributionsPage])

  const clearFilters = () => {
    setRepositoryId('')
    setFrom('')
    setTo('')
    setPage(1)
  }

  return (
    <div className="space-y-6">
      {/* Header + Filters */}
      <div className="card">
        <div>
          <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Auditoria</p>
          <h1 className="mt-1 text-3xl font-semibold text-on-surface">Contribuições</h1>
          <p className="mt-1 text-sm text-on-surface-variant">Commits e pull requests sincronizados com evidências auditáveis.</p>
        </div>

        <div className="mt-5 grid gap-3 md:grid-cols-3">
          <label className="text-xs text-on-surface-variant">
            Repositório
            <select
              className="mt-1.5 w-full px-3 py-2"
              value={repositoryId}
              onChange={event => setRepositoryId(event.target.value)}
              disabled={repositoriesQuery.isLoading || repositories.length === 0}
            >
              <option value="">Todos os repositórios</option>
              {repositories.map(repo => (
                <option key={repo.id} value={String(repo.id)}>
                  {repo.fullName}
                </option>
              ))}
            </select>
            {repositoriesQuery.isLoading && (
              <p className="mt-1 text-xs text-on-surface-variant">Carregando repositórios...</p>
            )}
            {repositoriesQuery.isError && (
              <p className="mt-1 text-xs text-tertiary">Não foi possível carregar o filtro.</p>
            )}
          </label>
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

        <button
          type="button"
          onClick={clearFilters}
          className="btn-ghost mt-2 text-xs py-0"
        >
          Limpar filtros
        </button>

        {/* Metrics row */}
        <div className="mt-4 grid gap-3 md:grid-cols-4">
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">Total</p>
            <p className="mt-1 text-xl font-semibold text-on-surface">{formatNumber(metrics.total)}</p>
          </div>
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">Commits</p>
            <p className="mt-1 text-xl font-semibold text-on-surface">{formatNumber(metrics.commits)}</p>
          </div>
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">Pull Requests</p>
            <p className="mt-1 text-xl font-semibold text-on-surface">{formatNumber(metrics.pullRequests)}</p>
          </div>
          <div className="metric-card">
            <p className="text-xs text-on-surface-variant">PRs aprovados</p>
            <p className="mt-1 text-xl font-semibold text-primary">{formatNumber(metrics.approvedPullRequests)}</p>
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="card">
        {contributionsQuery.isLoading && <p className="text-sm text-on-surface-variant">Carregando contribuições...</p>}
        {contributionsQuery.isError && <p className="text-sm text-error">Não foi possível carregar as contribuições.</p>}

        {!contributionsQuery.isLoading && !contributionsQuery.isError && contributions.length === 0 && (
          <p className="text-sm text-on-surface-variant">Nenhuma contribuição encontrada para os filtros informados.</p>
        )}

        {!contributionsQuery.isLoading && !contributionsQuery.isError && contributions.length > 0 && (
          <div className="space-y-4">
            <div className="overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead>
                  <tr className="text-xs text-on-surface-variant">
                    <th className="px-3 py-2 font-medium">Tipo</th>
                    <th className="px-3 py-2 font-medium">Título</th>
                    <th className="px-3 py-2 font-medium">Status</th>
                    <th className="px-3 py-2 font-medium">Autor</th>
                    <th className="px-3 py-2 font-medium">Repositório</th>
                    <th className="px-3 py-2 font-medium">Data</th>
                    <th className="px-3 py-2 font-medium">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {contributions.map(item => (
                    <tr key={item.id} className="border-t border-outline-variant/20 hover:bg-surface-container-highest/30 transition-colors">
                      <td className="px-3 py-2">
                        <span className={item.type === 'pull_request' ? 'chip-approved' : 'chip-neutral'}>
                          {item.type === 'pull_request' ? 'PR' : 'Commit'}
                        </span>
                      </td>
                      <td className="px-3 py-2 text-on-surface max-w-xs truncate">{item.title}</td>
                      <td className="px-3 py-2">
                        <span className={item.isApproved ? 'chip-approved' : 'chip-neutral'}>{item.status}</span>
                      </td>
                      <td className="px-3 py-2 text-on-surface-variant">{item.author}</td>
                      <td className="px-3 py-2 text-on-surface-variant font-mono text-xs">{item.repositoryFullName}</td>
                      <td className="px-3 py-2 text-on-surface-variant text-xs">{new Date(item.occurredAt).toLocaleString()}</td>
                      <td className="px-3 py-2">
                        <Link
                          to={item.type === 'pull_request' ? `/contributions/pull-requests/${item.id}` : `/contributions/commits/${item.id}`}
                          className="text-primary hover:underline text-xs"
                        >
                          Detalhar
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            <div className="flex flex-col gap-3 border-t border-outline-variant/20 pt-3 text-xs text-on-surface-variant md:flex-row md:items-center md:justify-between">
              <p>
                Página {contributionsPage?.page ?? 1} de {contributionsPage?.totalPages ?? 1} &middot; Total: {formatNumber(contributionsPage?.totalCount ?? 0)}
              </p>

              <div className="flex flex-wrap items-center gap-2">
                <label className="flex items-center gap-2 text-on-surface-variant">
                  <span>Itens por página</span>
                  <select
                    value={pageSize}
                    onChange={event => {
                      setPageSize(Number(event.target.value))
                      setPage(1)
                    }}
                    disabled={contributionsQuery.isFetching}
                    className="px-2 py-1.5 text-xs"
                  >
                    <option value={10}>10</option>
                    <option value={20}>20</option>
                    <option value={50}>50</option>
                  </select>
                </label>
                <button
                  type="button"
                  onClick={() => setPage(current => Math.max(1, current - 1))}
                  disabled={!contributionsPage?.hasPreviousPage || contributionsQuery.isFetching}
                  className="btn-secondary text-xs py-1.5"
                >
                  Anterior
                </button>
                <button
                  type="button"
                  onClick={() => setPage(current => current + 1)}
                  disabled={!contributionsPage?.hasNextPage || contributionsQuery.isFetching}
                  className="btn-secondary text-xs py-1.5"
                >
                  Próxima
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
