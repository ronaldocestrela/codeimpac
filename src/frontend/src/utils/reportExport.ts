import type { ExecutiveReport } from '../types/reports'

export type ReportExportFormat = 'markdown' | 'csv' | 'json'

function escapeCsv(value: string): string {
  const normalized = value.replace(/\r?\n/g, ' ')
  return `"${normalized.replace(/"/g, '""')}"`
}

function toSafeDate(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toISOString().replace(/[:]/g, '-').replace(/\.\d{3}Z$/, 'Z')
}

function buildEvidenceMarkdown(report: ExecutiveReport): string {
  if (report.evidence.length === 0) {
    return 'Nenhuma evidência registrada.'
  }

  const lines = report.evidence.map(item => (
    `- [${item.evidenceId}] ${item.evidenceType} | ${item.repositoryFullName} | ${item.author} | ${new Date(item.occurredAt).toLocaleString('pt-BR')} | ${item.status} | ${item.url}`
  ))

  return lines.join('\n')
}

export function buildExecutiveReportMarkdown(report: ExecutiveReport): string {
  const scope = report.scope
  const metrics = report.metrics

  const highlights = report.highlights.length > 0
    ? report.highlights.map(item => (
      `- **${item.title}**: ${item.insight}\n  - Impacto: ${item.impact}\n  - Evidências: ${item.evidenceIds.join(', ') || 'n/a'}`
    )).join('\n')
    : '- Nenhum highlight disponível.'

  const risks = report.risks.length > 0
    ? report.risks.map(item => (
      `- **Risco**: ${item.risk}\n  - Recomendação: ${item.recommendation}\n  - Evidências: ${item.evidenceIds.join(', ') || 'n/a'}`
    )).join('\n')
    : '- Nenhum risco disponível.'

  return [
    '# Relatório Executivo',
    '',
    `- ID: ${report.id}`,
    `- Gerado em: ${new Date(report.generatedAt).toLocaleString('pt-BR')}`,
    `- Escopo: ${scope.developerScope}`,
    `- Repositórios: ${scope.repositories.join(', ') || 'Todos'}`,
    `- Período: ${scope.from ?? '-'} até ${scope.to ?? '-'}`,
    '',
    '## Métricas',
    '',
    `- Commits: ${metrics.commitCount}`,
    `- PRs abertos: ${metrics.pullRequestOpenCount}`,
    `- PRs fechados: ${metrics.pullRequestClosedCount}`,
    `- PRs mergeados: ${metrics.pullRequestMergedCount}`,
    `- PRs aprovados: ${metrics.pullRequestApprovedCount}`,
    `- Lead time médio (h): ${metrics.averageMergeLeadTimeHours ?? '-'}`,
    '',
    '## Resumo Executivo',
    '',
    report.executiveSummary || 'Sem resumo disponível.',
    '',
    '## Highlights',
    '',
    highlights,
    '',
    '## Riscos e Próximos Passos',
    '',
    risks,
    '',
    '## Evidências',
    '',
    buildEvidenceMarkdown(report)
  ].join('\n')
}

export function buildExecutiveReportCsv(report: ExecutiveReport): string {
  const rows = [
    ['section', 'field', 'value'],
    ['meta', 'id', report.id],
    ['meta', 'generatedAt', report.generatedAt],
    ['scope', 'developerScope', report.scope.developerScope],
    ['scope', 'repositories', report.scope.repositories.join(', ')],
    ['scope', 'from', report.scope.from ?? ''],
    ['scope', 'to', report.scope.to ?? ''],
    ['metrics', 'commitCount', String(report.metrics.commitCount)],
    ['metrics', 'pullRequestOpenCount', String(report.metrics.pullRequestOpenCount)],
    ['metrics', 'pullRequestClosedCount', String(report.metrics.pullRequestClosedCount)],
    ['metrics', 'pullRequestMergedCount', String(report.metrics.pullRequestMergedCount)],
    ['metrics', 'pullRequestApprovedCount', String(report.metrics.pullRequestApprovedCount)],
    ['metrics', 'averageMergeLeadTimeHours', String(report.metrics.averageMergeLeadTimeHours ?? '')],
    ['summary', 'executiveSummary', report.executiveSummary]
  ]

  report.highlights.forEach((item, index) => {
    rows.push(['highlight', `title_${index + 1}`, item.title])
    rows.push(['highlight', `insight_${index + 1}`, item.insight])
    rows.push(['highlight', `impact_${index + 1}`, item.impact])
    rows.push(['highlight', `evidenceIds_${index + 1}`, item.evidenceIds.join(', ')])
  })

  report.risks.forEach((item, index) => {
    rows.push(['risk', `risk_${index + 1}`, item.risk])
    rows.push(['risk', `recommendation_${index + 1}`, item.recommendation])
    rows.push(['risk', `evidenceIds_${index + 1}`, item.evidenceIds.join(', ')])
  })

  report.evidence.forEach((item, index) => {
    rows.push(['evidence', `id_${index + 1}`, item.evidenceId])
    rows.push(['evidence', `type_${index + 1}`, item.evidenceType])
    rows.push(['evidence', `repository_${index + 1}`, item.repositoryFullName])
    rows.push(['evidence', `reference_${index + 1}`, item.externalReference])
    rows.push(['evidence', `author_${index + 1}`, item.author])
    rows.push(['evidence', `occurredAt_${index + 1}`, item.occurredAt])
    rows.push(['evidence', `status_${index + 1}`, item.status])
    rows.push(['evidence', `url_${index + 1}`, item.url])
  })

  return rows.map(row => row.map(escapeCsv).join(',')).join('\n')
}

export function getReportExportFileName(report: ExecutiveReport, format: ReportExportFormat): string {
  const generatedAt = toSafeDate(report.generatedAt)
  return `codeimpact-report-${generatedAt}.${format === 'markdown' ? 'md' : format}`
}

export function downloadExport(content: string, fileName: string, mimeType: string): void {
  const blob = new Blob([content], { type: mimeType })
  const href = URL.createObjectURL(blob)
  const anchor = document.createElement('a')

  anchor.href = href
  anchor.download = fileName
  document.body.appendChild(anchor)
  anchor.click()
  document.body.removeChild(anchor)

  URL.revokeObjectURL(href)
}
