import { useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { generateExecutiveReport, getExecutiveReportDetail, getExecutiveReports } from '../services/reports'
import type { ExecutiveReport, ExecutiveReportFilters, ExecutiveReportListItem } from '../types/reports'

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return '-'
  }

  return new Date(value).toLocaleString()
}

function ListItemButton({
  item,
  selected,
  onSelect
}: {
  item: ExecutiveReportListItem
  selected: boolean
  onSelect: (id: string) => void
}) {
  return (
    <button
      type="button"
      onClick={() => onSelect(item.id)}
      className={`w-full rounded-lg border px-4 py-3 text-left transition ${selected ? 'border-indigo-500 bg-indigo-50' : 'border-slate-200 bg-white hover:bg-slate-50'}`}
    >
      <p className="text-sm font-semibold text-slate-800">{new Date(item.generatedAt).toLocaleString()}</p>
      <p className="mt-1 text-xs text-slate-500">Commits: {item.commitCount} | PRs aprovados: {item.pullRequestApprovedCount} | Repositórios: {item.repositoryCount}</p>
      <p className="mt-2 text-sm text-slate-700">{item.executiveSummaryPreview || 'Sem resumo disponível.'}</p>
    </button>
  )
}

export default function ReportsPage() {
  const [repositoryId, setRepositoryId] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [selectedReportId, setSelectedReportId] = useState<string | null>(null)

  const filters = useMemo<ExecutiveReportFilters>(() => ({
    repositoryId: repositoryId ? Number(repositoryId) : undefined,
    from: from || undefined,
    to: to || undefined
  }), [repositoryId, from, to])

  const reportsQuery = useQuery({
    queryKey: ['reports', filters],
    queryFn: () => getExecutiveReports(filters)
  })

  const detailQuery = useQuery({
    queryKey: ['report', selectedReportId],
    queryFn: () => getExecutiveReportDetail(selectedReportId ?? ''),
    enabled: Boolean(selectedReportId)
  })

  const createMutation = useMutation({
    mutationFn: () => generateExecutiveReport(filters),
    onSuccess: (report: ExecutiveReport) => {
      setSelectedReportId(report.id)
      void reportsQuery.refetch()
    }
  })

  const reports = reportsQuery.data ?? []

  return (
    <div className="space-y-6">
      <section className="rounded-lg border bg-white p-4 shadow-sm">
        <h1 className="text-2xl font-semibold text-slate-900">Relatórios Executivos</h1>
        <p className="mt-1 text-sm text-slate-600">Gere e consulte relatórios orientados à liderança com métricas e evidências rastreáveis.</p>

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

        <button
          type="button"
          onClick={() => createMutation.mutate()}
          disabled={createMutation.isPending}
          className="mt-4 rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {createMutation.isPending ? 'Gerando relatório...' : 'Gerar Relatório'}
        </button>

        {createMutation.isError && (
          <p className="mt-3 text-sm text-red-600">Não foi possível gerar o relatório com os filtros informados.</p>
        )}
      </section>

      <div className="grid gap-6 lg:grid-cols-[340px_1fr]">
        <section className="space-y-3 rounded-lg border bg-white p-4 shadow-sm">
          <h2 className="text-base font-semibold text-slate-900">Histórico</h2>

          {reportsQuery.isLoading && <p className="text-sm text-slate-600">Carregando relatórios...</p>}
          {reportsQuery.isError && <p className="text-sm text-red-600">Falha ao carregar histórico de relatórios.</p>}
          {!reportsQuery.isLoading && !reportsQuery.isError && reports.length === 0 && (
            <p className="text-sm text-slate-600">Nenhum relatório encontrado para os filtros informados.</p>
          )}

          {!reportsQuery.isLoading && !reportsQuery.isError && reports.map(item => (
            <ListItemButton
              key={item.id}
              item={item}
              selected={item.id === selectedReportId}
              onSelect={setSelectedReportId}
            />
          ))}
        </section>

        <section className="rounded-lg border bg-white p-4 shadow-sm">
          {!selectedReportId && <p className="text-sm text-slate-600">Selecione um relatório no histórico para visualizar os detalhes.</p>}
          {selectedReportId && detailQuery.isLoading && <p className="text-sm text-slate-600">Carregando detalhes do relatório...</p>}
          {selectedReportId && detailQuery.isError && <p className="text-sm text-red-600">Falha ao carregar detalhes do relatório.</p>}

          {detailQuery.data && (
            <div className="space-y-5">
              <div>
                <h2 className="text-xl font-semibold text-slate-900">Relatório Executivo</h2>
                <p className="mt-1 text-sm text-slate-500">Gerado em {formatDate(detailQuery.data.generatedAt)}</p>
              </div>

              <div className="grid gap-3 text-sm md:grid-cols-2 xl:grid-cols-3">
                <div className="rounded border border-slate-200 bg-slate-50 p-3"><strong>Commits:</strong> {detailQuery.data.metrics.commitCount}</div>
                <div className="rounded border border-slate-200 bg-slate-50 p-3"><strong>PRs abertos:</strong> {detailQuery.data.metrics.pullRequestOpenCount}</div>
                <div className="rounded border border-slate-200 bg-slate-50 p-3"><strong>PRs fechados:</strong> {detailQuery.data.metrics.pullRequestClosedCount}</div>
                <div className="rounded border border-slate-200 bg-slate-50 p-3"><strong>PRs mergeados:</strong> {detailQuery.data.metrics.pullRequestMergedCount}</div>
                <div className="rounded border border-slate-200 bg-slate-50 p-3"><strong>PRs aprovados:</strong> {detailQuery.data.metrics.pullRequestApprovedCount}</div>
                <div className="rounded border border-slate-200 bg-slate-50 p-3"><strong>Lead time médio:</strong> {detailQuery.data.metrics.averageMergeLeadTimeHours ?? '-'} h</div>
              </div>

              <div>
                <h3 className="text-base font-semibold text-slate-900">Resumo Executivo</h3>
                <p className="mt-2 whitespace-pre-wrap text-sm text-slate-700">{detailQuery.data.executiveSummary || 'Sem resumo gerado.'}</p>
              </div>

              <div>
                <h3 className="text-base font-semibold text-slate-900">Highlights</h3>
                {detailQuery.data.highlights.length === 0 && <p className="mt-2 text-sm text-slate-600">Nenhum highlight retornado.</p>}
                <div className="mt-2 space-y-3">
                  {detailQuery.data.highlights.map(item => (
                    <article key={`${item.title}-${item.impact}`} className="rounded border border-slate-200 p-3">
                      <p className="font-medium text-slate-900">{item.title}</p>
                      <p className="mt-1 text-sm text-slate-700">{item.insight}</p>
                      <p className="mt-1 text-sm text-slate-600"><strong>Impacto:</strong> {item.impact}</p>
                      <p className="mt-1 text-xs text-slate-500">Evidências: {item.evidenceIds.join(', ') || 'n/a'}</p>
                    </article>
                  ))}
                </div>
              </div>

              <div>
                <h3 className="text-base font-semibold text-slate-900">Riscos e Próximos Passos</h3>
                {detailQuery.data.risks.length === 0 && <p className="mt-2 text-sm text-slate-600">Nenhum risco sinalizado.</p>}
                <div className="mt-2 space-y-3">
                  {detailQuery.data.risks.map(item => (
                    <article key={`${item.risk}-${item.recommendation}`} className="rounded border border-amber-200 bg-amber-50 p-3">
                      <p className="text-sm font-medium text-amber-900">{item.risk}</p>
                      <p className="mt-1 text-sm text-amber-800"><strong>Recomendação:</strong> {item.recommendation}</p>
                      <p className="mt-1 text-xs text-amber-700">Evidências: {item.evidenceIds.join(', ') || 'n/a'}</p>
                    </article>
                  ))}
                </div>
              </div>
            </div>
          )}
        </section>
      </div>
    </div>
  )
}
