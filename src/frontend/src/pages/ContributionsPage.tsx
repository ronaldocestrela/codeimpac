import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { getContributions } from '../services/contributions'
import type { ContributionListItem } from '../types/contributions'

export default function ContributionsPage() {
  const [contributions, setContributions] = useState<ContributionListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [repositoryId, setRepositoryId] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')

  const filters = useMemo(() => ({
    repositoryId: repositoryId ? Number(repositoryId) : undefined,
    from: from || undefined,
    to: to || undefined
  }), [repositoryId, from, to])

  useEffect(() => {
    setLoading(true)
    setError(null)

    getContributions(filters)
      .then(setContributions)
      .catch(() => setError('Não foi possível carregar as contribuições.'))
      .finally(() => setLoading(false))
  }, [filters])

  return (
    <div className="space-y-6">
      <section className="rounded-lg border bg-white p-4 shadow-sm">
        <h1 className="text-2xl font-semibold text-slate-900">Contribuições</h1>
        <p className="mt-1 text-sm text-slate-600">Visualize commits e pull requests sincronizados com evidências auditáveis.</p>

        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <label className="text-sm text-slate-700">
            Repositório (ID)
            <input
              className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
              placeholder="Ex: 100"
              value={repositoryId}
              onChange={event => setRepositoryId(event.target.value)}
            />
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
      </section>

      <section className="rounded-lg border bg-white p-4 shadow-sm">
        {loading && <p className="text-sm text-slate-600">Carregando contribuições...</p>}
        {error && <p className="text-sm text-red-600">{error}</p>}

        {!loading && !error && contributions.length === 0 && (
          <p className="text-sm text-slate-600">Nenhuma contribuição encontrada para os filtros informados.</p>
        )}

        {!loading && !error && contributions.length > 0 && (
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
