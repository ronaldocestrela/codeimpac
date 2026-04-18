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
        <div>
          <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Detalhe</p>
          <h1 className="mt-1 text-3xl font-semibold text-on-surface">Contribuição</h1>
        </div>
        <Link to="/contributions" className="btn-ghost text-xs">← Voltar</Link>
      </div>

      {loading && <p className="text-sm text-on-surface-variant">Carregando detalhes...</p>}
      {error && <p className="text-sm text-error">{error}</p>}

      {!loading && !error && contribution && (
        <>
          <div className="card">
            <h2 className="text-lg font-semibold text-on-surface">{contribution.title}</h2>
            <div className="mt-4 grid gap-3 text-sm md:grid-cols-2">
              <div className="metric-card">
                <p className="text-xs text-on-surface-variant">Tipo</p>
                <p className="mt-1 text-on-surface font-medium">
                  {contribution.type === 'pull_request' ? 'Pull Request' : 'Commit'}
                </p>
              </div>
              <div className="metric-card">
                <p className="text-xs text-on-surface-variant">Status</p>
                <p className="mt-1">
                  <span className={contribution.isApproved ? 'chip-approved' : 'chip-neutral'}>{contribution.status}</span>
                </p>
              </div>
              <div className="metric-card">
                <p className="text-xs text-on-surface-variant">Autor</p>
                <p className="mt-1 text-on-surface font-medium">{contribution.author}</p>
              </div>
              <div className="metric-card">
                <p className="text-xs text-on-surface-variant">Repositório</p>
                <p className="mt-1 text-on-surface font-mono text-xs">{contribution.repositoryFullName}</p>
              </div>
              <div className="metric-card">
                <p className="text-xs text-on-surface-variant">Referência</p>
                <p className="mt-1 text-on-surface font-mono text-xs">{contribution.externalReference}</p>
              </div>
              <div className="metric-card">
                <p className="text-xs text-on-surface-variant">Data</p>
                <p className="mt-1 text-on-surface text-xs">{new Date(contribution.occurredAt).toLocaleString()}</p>
              </div>
            </div>
            <a
              className="mt-4 inline-flex items-center gap-1 text-sm text-primary hover:underline"
              href={contribution.url}
              target="_blank"
              rel="noreferrer"
            >
              Abrir evidência no GitHub ↗
            </a>
          </div>

          <div className="card">
            <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Evidências rastreáveis</p>
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead>
                  <tr className="text-xs text-on-surface-variant">
                    <th className="px-3 py-2 font-medium">Tipo</th>
                    <th className="px-3 py-2 font-medium">Referência</th>
                    <th className="px-3 py-2 font-medium">Ator</th>
                    <th className="px-3 py-2 font-medium">Estado</th>
                    <th className="px-3 py-2 font-medium">Data</th>
                    <th className="px-3 py-2 font-medium">Link</th>
                  </tr>
                </thead>
                <tbody>
                  {contribution.evidence.map(item => (
                    <tr key={`${item.evidenceType}-${item.externalReference}-${item.url}`} className="border-t border-outline-variant/20 hover:bg-surface-container-highest/30 transition-colors">
                      <td className="px-3 py-2"><span className="chip-neutral">{item.evidenceType}</span></td>
                      <td className="px-3 py-2 font-mono text-xs text-on-surface-variant">{item.externalReference}</td>
                      <td className="px-3 py-2 text-on-surface-variant">{item.actor}</td>
                      <td className="px-3 py-2"><span className={item.state === 'APPROVED' ? 'chip-approved' : 'chip-neutral'}>{item.state}</span></td>
                      <td className="px-3 py-2 text-xs text-on-surface-variant">{new Date(item.occurredAt).toLocaleString()}</td>
                      <td className="px-3 py-2">
                        <a href={item.url} target="_blank" rel="noreferrer" className="text-primary hover:underline text-xs">Abrir ↗</a>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
