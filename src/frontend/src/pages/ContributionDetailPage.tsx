import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getCommitContributionDetail, getPullRequestContributionDetail } from '../services/contributions'
import type { ContributionDetail } from '../types/contributions'

export default function ContributionDetailPage() {
  const params = useParams<{ type: string, id: string }>()
  const [contribution, setContribution] = useState<ContributionDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const contributionId = params.id
    const contributionType = params.type

    if (!contributionId || !contributionType) {
      setError('Contribuição inválida.')
      setLoading(false)
      return
    }

    setLoading(true)
    setError(null)

    const request = contributionType === 'pull-requests'
      ? getPullRequestContributionDetail(contributionId)
      : getCommitContributionDetail(contributionId)

    request
      .then(setContribution)
      .catch(() => setError('Não foi possível carregar os detalhes da contribuição.'))
      .finally(() => setLoading(false))
  }, [params.id, params.type])

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-slate-900">Detalhe da Contribuição</h1>
        <Link to="/contributions" className="text-indigo-600 hover:underline">Voltar</Link>
      </div>

      {loading && <p className="text-sm text-slate-600">Carregando detalhes...</p>}
      {error && <p className="text-sm text-red-600">{error}</p>}

      {!loading && !error && contribution && (
        <>
          <section className="rounded-lg border bg-white p-4 shadow-sm">
            <h2 className="text-lg font-semibold text-slate-900">{contribution.title}</h2>
            <div className="mt-3 grid gap-2 text-sm text-slate-700 md:grid-cols-2">
              <p><strong>Tipo:</strong> {contribution.type === 'pull_request' ? 'Pull Request' : 'Commit'}</p>
              <p><strong>Status:</strong> {contribution.status}</p>
              <p><strong>Autor:</strong> {contribution.author}</p>
              <p><strong>Repositório:</strong> {contribution.repositoryFullName}</p>
              <p><strong>Referência:</strong> {contribution.externalReference}</p>
              <p><strong>Data:</strong> {new Date(contribution.occurredAt).toLocaleString()}</p>
            </div>
            <a
              className="mt-4 inline-block text-indigo-600 hover:underline"
              href={contribution.url}
              target="_blank"
              rel="noreferrer"
            >
              Abrir evidência no GitHub
            </a>
          </section>

          <section className="rounded-lg border bg-white p-4 shadow-sm">
            <h3 className="text-base font-semibold text-slate-900">Evidências</h3>
            <div className="mt-3 overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead>
                  <tr className="border-b text-slate-500">
                    <th className="px-3 py-2">Tipo</th>
                    <th className="px-3 py-2">Referência</th>
                    <th className="px-3 py-2">Ator</th>
                    <th className="px-3 py-2">Estado</th>
                    <th className="px-3 py-2">Data</th>
                    <th className="px-3 py-2">Link</th>
                  </tr>
                </thead>
                <tbody>
                  {contribution.evidence.map(item => (
                    <tr key={`${item.evidenceType}-${item.externalReference}-${item.url}`} className="border-b last:border-0">
                      <td className="px-3 py-2">{item.evidenceType}</td>
                      <td className="px-3 py-2">{item.externalReference}</td>
                      <td className="px-3 py-2">{item.actor}</td>
                      <td className="px-3 py-2">{item.state}</td>
                      <td className="px-3 py-2">{new Date(item.occurredAt).toLocaleString()}</td>
                      <td className="px-3 py-2">
                        <a href={item.url} target="_blank" rel="noreferrer" className="text-indigo-600 hover:underline">Abrir</a>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        </>
      )}
    </div>
  )
}
