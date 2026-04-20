import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { enqueueExecutiveReportGeneration, exportExecutiveReport, getExecutiveReportDetail, getExecutiveReports } from '../services/reports'
import { getBackgroundJobStatus } from '../services/backgroundJobs'
import { getGitHubRepositories, getSelectedGitHubRepositories } from '../services/github'
import type { ExecutiveReportFilters, ExecutiveReportListItem } from '../types/reports'
import type { GitHubRepository } from '../types/github'
import type { BackgroundJobStatusResponse } from '../types/backgroundJobs'
import {
  downloadBlobExport,
  extractFileNameFromContentDisposition,
  getReportExportFallbackFileName,
  type ReportExportFormat
} from '../utils/reportExport'

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
      className={`w-full rounded-md px-4 py-3 text-left transition ${
        selected
          ? 'bg-primary/10 border border-primary/30'
          : 'bg-surface-container hover:bg-surface-container-highest border border-transparent'
      }`}
    >
      <p className="text-xs font-semibold text-on-surface">{new Date(item.generatedAt).toLocaleString()}</p>
      <p className="mt-1 text-xs text-on-surface-variant">
        Commits: {item.commitCount} &middot; PRs aprovados: {item.pullRequestApprovedCount} &middot; Repos: {item.repositoryCount}
      </p>
      <p className="mt-2 text-xs text-on-surface-variant line-clamp-2">{item.executiveSummaryPreview || 'Sem resumo disponível.'}</p>
    </button>
  )
}

export default function ReportsPage() {
  const [organizationLogin, setOrganizationLogin] = useState('')
  const [repositoryId, setRepositoryId] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [selectedReportId, setSelectedReportId] = useState<string | null>(null)
  const [pendingJobId, setPendingJobId] = useState<string | null>(null)
  const [jobFeedback, setJobFeedback] = useState<string | null>(null)
  const [exportFormat, setExportFormat] = useState<ReportExportFormat>('markdown')
  const [exportFeedback, setExportFeedback] = useState<string | null>(null)

  const repositoriesQuery = useQuery({
    queryKey: ['report-repositories'],
    queryFn: async (): Promise<GitHubRepository[]> => {
      const selectedRepositories = await getSelectedGitHubRepositories()
      if (selectedRepositories.length > 0) {
        return selectedRepositories
      }

      return getGitHubRepositories()
    }
  })

  const filters = useMemo<ExecutiveReportFilters>(() => ({
    repositoryId: repositoryId ? Number(repositoryId) : undefined,
    organizationLogin: organizationLogin || undefined,
    from: from || undefined,
    to: to || undefined
  }), [repositoryId, organizationLogin, from, to])

  const organizations = useMemo(() => {
    const owners = new Set<string>()
    ;(repositoriesQuery.data ?? []).forEach(repo => {
      if (repo.ownerLogin) {
        owners.add(repo.ownerLogin)
      }
    })

    return Array.from(owners).sort((a, b) => a.localeCompare(b, 'pt-BR'))
  }, [repositoriesQuery.data])

  const reportsQuery = useQuery({
    queryKey: ['reports', filters],
    queryFn: () => getExecutiveReports(filters)
  })

  const detailQuery = useQuery({
    queryKey: ['report', selectedReportId],
    queryFn: () => getExecutiveReportDetail(selectedReportId ?? ''),
    enabled: Boolean(selectedReportId)
  })

  const jobStatusQuery = useQuery({
    queryKey: ['report-generation-job', pendingJobId],
    queryFn: (): Promise<BackgroundJobStatusResponse> => getBackgroundJobStatus(pendingJobId ?? ''),
    enabled: Boolean(pendingJobId),
    refetchInterval: data => {
      if (!data) {
        return 2000
      }

      const status = data?.status
      return status === 'Queued' || status === 'Processing' ? 2000 : false
    }
  })

  useEffect(() => {
    if (!pendingJobId || !jobStatusQuery.data) {
      return
    }

    if (jobStatusQuery.data.status === 'Succeeded') {
      if (jobStatusQuery.data.reportId) {
        setSelectedReportId(jobStatusQuery.data.reportId)
        setJobFeedback('Relatorio concluido com sucesso.')
      } else {
        setJobFeedback('Relatorio concluido, mas sem identificador para abrir automaticamente.')
      }

      setPendingJobId(null)
      void reportsQuery.refetch()
      return
    }

    if (jobStatusQuery.data.status === 'Failed') {
      setPendingJobId(null)
      setJobFeedback(jobStatusQuery.data.errorMessage || 'Falha ao processar o relatorio em background.')
    }
  }, [pendingJobId, jobStatusQuery.data])

  const createMutation = useMutation({
    mutationFn: () => enqueueExecutiveReportGeneration(filters),
    onSuccess: job => {
      setPendingJobId(job.taskId)
      setJobFeedback('Geracao enfileirada. Processando em background...')
    }
  })

  const reports = reportsQuery.data ?? []
  const repositories = useMemo(() => {
    const allRepositories = repositoriesQuery.data ?? []
    if (!organizationLogin) {
      return allRepositories
    }

    return allRepositories.filter(repo => repo.ownerLogin === organizationLogin)
  }, [repositoriesQuery.data, organizationLogin])

  const handleExport = async () => {
    if (!detailQuery.data) {
      return
    }

    const report = detailQuery.data
    try {
      const exportResponse = await exportExecutiveReport(report.id, exportFormat)
      const serverFileName = extractFileNameFromContentDisposition(exportResponse.contentDisposition)
      const fallbackFileName = getReportExportFallbackFileName(report.generatedAt, exportFormat)
      downloadBlobExport(exportResponse.blob, serverFileName ?? fallbackFileName)

      if (exportFormat === 'markdown') {
        setExportFeedback('Relatorio exportado em Markdown.')
      } else if (exportFormat === 'pdf') {
        setExportFeedback('Relatorio exportado em PDF.')
      } else {
        setExportFeedback('Relatorio exportado em DOCX.')
      }
    } catch {
      setExportFeedback('Falha ao exportar relatorio.')
    }
  }

  return (
    <div className="space-y-6">
      {/* Header + filters + generate */}
      <div className="card">
        <div>
          <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Liderança</p>
          <h1 className="mt-1 text-3xl font-semibold text-on-surface">Relatórios Executivos</h1>
          <p className="mt-1 text-sm text-on-surface-variant">Gere e consulte relatórios orientados à liderança com métricas e evidências rastreáveis.</p>
        </div>

        <div className="mt-5 grid gap-3 md:grid-cols-4">
          <label className="text-xs text-on-surface-variant">
            Organização
            <select
              className="mt-1.5 w-full px-3 py-2"
              value={organizationLogin}
              onChange={event => {
                setOrganizationLogin(event.target.value)
                setRepositoryId('')
              }}
              disabled={repositoriesQuery.isLoading || organizations.length === 0}
            >
              <option value="">Todas as organizações</option>
              {organizations.map(owner => (
                <option key={owner} value={owner}>
                  {owner}
                </option>
              ))}
            </select>
          </label>

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
              <p className="mt-1 text-xs text-error">Não foi possível carregar os repositórios.</p>
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

        <div className="mt-4 flex flex-wrap items-center gap-3">
          <button
            type="button"
            onClick={() => createMutation.mutate()}
            disabled={createMutation.isPending || Boolean(pendingJobId)}
            className="btn-primary"
          >
            {createMutation.isPending || pendingJobId ? 'Processando...' : 'Gerar Relatório'}
          </button>

          {createMutation.isError && (
            <p className="text-xs text-error">Não foi possível gerar o relatório.</p>
          )}
          {jobFeedback && (
            <p className={`text-xs ${
              jobFeedback.toLowerCase().includes('falha') ? 'text-error' : 'text-on-surface-variant'
            }`}>{jobFeedback}</p>
          )}
          {pendingJobId && jobStatusQuery.data?.status === 'Processing' && (
            <p className="text-xs text-on-surface-variant">Job em execução. Atualizando automaticamente...</p>
          )}
        </div>
      </div>

      {/* Two-column layout: history + detail */}
      <div className="grid gap-6 lg:grid-cols-[320px_1fr]">
        {/* History list */}
        <div className="card space-y-2">
          <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Histórico</p>

          {reportsQuery.isLoading && <p className="text-sm text-on-surface-variant">Carregando relatórios...</p>}
          {reportsQuery.isError && <p className="text-sm text-error">Falha ao carregar histórico.</p>}
          {!reportsQuery.isLoading && !reportsQuery.isError && reports.length === 0 && (
            <p className="text-sm text-on-surface-variant">Nenhum relatório encontrado.</p>
          )}

          <div className="mt-2 space-y-2">
            {!reportsQuery.isLoading && !reportsQuery.isError && reports.map(item => (
              <ListItemButton
                key={item.id}
                item={item}
                selected={item.id === selectedReportId}
                onSelect={setSelectedReportId}
              />
            ))}
          </div>
        </div>

        {/* Report detail */}
        <div className="card">
          {/* Export toolbar */}
          <div className="mb-5 flex flex-wrap items-center gap-2 pb-4 border-b border-outline-variant/20">
            <select
              className="px-3 py-2 text-xs"
              value={exportFormat}
              onChange={event => setExportFormat(event.target.value as ReportExportFormat)}
              disabled={!detailQuery.data}
            >
              <option value="markdown">Markdown (.md)</option>
              <option value="pdf">PDF (.pdf)</option>
              <option value="docx">DOCX (.docx)</option>
            </select>

            <button
              type="button"
              onClick={handleExport}
              disabled={!detailQuery.data}
              className="btn-secondary text-xs"
            >
              Exportar
            </button>

            {exportFeedback && (
              <p className={`text-xs ${
                exportFeedback.toLowerCase().includes('falha') ? 'text-error' : 'text-on-surface-variant'
              }`}>{exportFeedback}</p>
            )}
          </div>

          {!selectedReportId && (
            <p className="text-sm text-on-surface-variant">Selecione um relatório no histórico para visualizar os detalhes.</p>
          )}
          {selectedReportId && detailQuery.isLoading && (
            <p className="text-sm text-on-surface-variant">Carregando detalhes...</p>
          )}
          {selectedReportId && detailQuery.isError && (
            <p className="text-sm text-error">Falha ao carregar detalhes do relatório.</p>
          )}

          {detailQuery.data && (
            <div className="space-y-6">
              <div>
                <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Relatório Executivo</p>
                <p className="mt-1 text-xs text-on-surface-variant">Gerado em {formatDate(detailQuery.data.generatedAt)}</p>
              </div>

              {/* Metrics */}
              <div className="grid gap-3 md:grid-cols-3">
                <div className="metric-card">
                  <p className="text-xs text-on-surface-variant">Commits</p>
                  <p className="mt-1 text-xl font-semibold text-on-surface">{detailQuery.data.metrics.commitCount}</p>
                </div>
                <div className="metric-card">
                  <p className="text-xs text-on-surface-variant">PRs abertos</p>
                  <p className="mt-1 text-xl font-semibold text-on-surface">{detailQuery.data.metrics.pullRequestOpenCount}</p>
                </div>
                <div className="metric-card">
                  <p className="text-xs text-on-surface-variant">PRs fechados</p>
                  <p className="mt-1 text-xl font-semibold text-on-surface">{detailQuery.data.metrics.pullRequestClosedCount}</p>
                </div>
                <div className="metric-card">
                  <p className="text-xs text-on-surface-variant">PRs mergeados</p>
                  <p className="mt-1 text-xl font-semibold text-on-surface">{detailQuery.data.metrics.pullRequestMergedCount}</p>
                </div>
                <div className="metric-card">
                  <p className="text-xs text-on-surface-variant">PRs aprovados</p>
                  <p className="mt-1 text-xl font-semibold text-primary">{detailQuery.data.metrics.pullRequestApprovedCount}</p>
                </div>
                <div className="metric-card">
                  <p className="text-xs text-on-surface-variant">Lead time médio</p>
                  <p className="mt-1 text-xl font-semibold text-on-surface">{detailQuery.data.metrics.averageMergeLeadTimeHours ?? '-'} h</p>
                </div>
              </div>

              {/* Executive summary */}
              <div>
                <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Resumo Executivo</p>
                <p className="mt-3 whitespace-pre-wrap text-sm text-on-surface leading-relaxed">
                  {detailQuery.data.executiveSummary || 'Sem resumo gerado.'}
                </p>
              </div>

              {/* Highlights */}
              <div>
                <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Highlights</p>
                {detailQuery.data.highlights.length === 0 && (
                  <p className="mt-2 text-sm text-on-surface-variant">Nenhum highlight retornado.</p>
                )}
                <div className="mt-3 space-y-3">
                  {detailQuery.data.highlights.map(item => (
                    <article key={`${item.title}-${item.impact}`} className="metric-card">
                      <p className="font-semibold text-on-surface text-sm">{item.title}</p>
                      <p className="mt-1 text-sm text-on-surface-variant">{item.insight}</p>
                      <p className="mt-1 text-xs text-on-surface-variant"><strong className="text-on-surface">Impacto:</strong> {item.impact}</p>
                      <p className="mt-1 text-xs text-on-surface-variant/60">Evidências: {item.evidenceIds.join(', ') || 'n/a'}</p>
                    </article>
                  ))}
                </div>
              </div>

              {/* Risks */}
              <div>
                <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Riscos e Próximos Passos</p>
                {detailQuery.data.risks.length === 0 && (
                  <p className="mt-2 text-sm text-on-surface-variant">Nenhum risco sinalizado.</p>
                )}
                <div className="mt-3 space-y-3">
                  {detailQuery.data.risks.map(item => (
                    <article key={`${item.risk}-${item.recommendation}`} className="bg-tertiary/5 border border-tertiary/20 rounded-md p-3">
                      <p className="text-sm font-semibold text-tertiary">{item.risk}</p>
                      <p className="mt-1 text-sm text-on-surface-variant"><strong className="text-on-surface">Recomendação:</strong> {item.recommendation}</p>
                      <p className="mt-1 text-xs text-on-surface-variant/60">Evidências: {item.evidenceIds.join(', ') || 'n/a'}</p>
                    </article>
                  ))}
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
