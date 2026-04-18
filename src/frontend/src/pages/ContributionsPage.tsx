import { useMemo, useState } from 'react'
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

  const filters = useMemo(() => ({
    repositoryId: repositoryId ? Number(repositoryId) : undefined,
    from: from || undefined,
    to: to || undefined
  }), [repositoryId, from, to])

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

  const contributions = contributionsQuery.data ?? []
  const repositories = repositoriesQuery.data ?? []

  const metrics = useMemo(() => {
    const commits = contributions.filter(item => item.type === 'commit').length
    const pullRequests = contributions.filter(item => item.type === 'pull_request').length
    const approvedPullRequests = contributions.filter(item => item.type === 'pull_request' && item.isApproved === true).length

    return {
      total: contributions.length,
      commits,
      pullRequests,
      approvedPullRequests
    }
  }, [contributions])

  const clearFilters = () => {
    setRepositoryId('')
    setFrom('')
    setTo('')
  }

  return (
    <div className="space-y-6">
      <section className="rounded-lg border bg-white p-4 shadow-sm">
        <h1 className="text-2xl font-semibold text-slate-900">Contribuições</h1>
        <p className="mt-1 text-sm text-slate-600">Visualize commits e pull requests sincronizados com evidências auditáveis.</p>

        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <label className="text-sm text-slate-700">
            Repositório
            <select
              className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
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
              <p className="mt-1 text-xs text-slate-500">Carregando repositórios...</p>
            )}
            {repositoriesQuery.isError && (
              <p className="mt-1 text-xs text-amber-700">Não foi possível carregar o filtro de repositórios.</p>
            )}
          </label>
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

        <button
          type="button"
          onClick={clearFilters}
          className="mt-3 text-sm text-indigo-700 hover:underline"
        >
          Limpar filtros
        </button>

        <div className="mt-4 grid gap-3 md:grid-cols-4">
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">Total</p>
            <p className="mt-1 text-lg font-semibold text-slate-900">{formatNumber(metrics.total)}</p>
          </div>
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">Commits</p>
            <p className="mt-1 text-lg font-semibold text-slate-900">{formatNumber(metrics.commits)}</p>
          </div>
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">Pull Requests</p>
            <p className="mt-1 text-lg font-semibold text-slate-900">{formatNumber(metrics.pullRequests)}</p>
          </div>
          <div className="rounded border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-500">PRs aprovados</p>
            <p className="mt-1 text-lg font-semibold text-slate-900">{formatNumber(metrics.approvedPullRequests)}</p>
          </div>
        </div>
      </section>

      <section className="rounded-lg border bg-white p-4 shadow-sm">
        {contributionsQuery.isLoading && <p className="text-sm text-slate-600">Carregando contribuições...</p>}
        {contributionsQuery.isError && <p className="text-sm text-red-600">Não foi possível carregar as contribuições.</p>}

        {!contributionsQuery.isLoading && !contributionsQuery.isError && contributions.length === 0 && (
          <p className="text-sm text-slate-600">Nenhuma contribuição encontrada para os filtros informados.</p>
        )}

        {!contributionsQuery.isLoading && !contributionsQuery.isError && contributions.length > 0 && (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead>
                <tr className="border-b text-slate-500">
                  <th className="px-3 py-2">Tipo</th>
                  <th className="px-3 py-2">Título</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2">Autor</th>
                  <th className="px-3 py-2">Repositório</th>
                  <th className="px-3 py-2">Data</th>
                  <th className="px-3 py-2">Ações</th>
                </tr>
              </thead>
              <tbody>
                {contributions.map(item => (
                  <tr key={item.id} className="border-b last:border-0">
                    <td className="px-3 py-2">{item.type === 'pull_request' ? 'PR' : 'Commit'}</td>
                    <td className="px-3 py-2">{item.title}</td>
                    <td className="px-3 py-2">{item.status}</td>
                    <td className="px-3 py-2">{item.author}</td>
                    <td className="px-3 py-2">{item.repositoryFullName}</td>
                    <td className="px-3 py-2">{new Date(item.occurredAt).toLocaleString()}</td>
                    <td className="px-3 py-2">
                      <Link
                        to={item.type === 'pull_request' ? `/contributions/pull-requests/${item.id}` : `/contributions/commits/${item.id}`}
                        className="text-indigo-600 hover:underline"
                      >
                        Detalhar
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
