import {
  buildExecutiveReportCsv,
  buildExecutiveReportMarkdown,
  getReportExportFileName
} from './reportExport'
import type { ExecutiveReport } from '../types/reports'

const reportFixture: ExecutiveReport = {
  id: 'fdb89f46-6b68-4ce3-8f45-fce95af0f887',
  generatedAt: '2026-04-18T12:30:00Z',
  scope: {
    developerScope: 'john.doe@company.com',
    repositoryId: 123,
    from: '2026-03-01T00:00:00Z',
    to: '2026-03-31T23:59:59Z',
    repositories: ['org/repo-a']
  },
  metrics: {
    commitCount: 12,
    pullRequestOpenCount: 2,
    pullRequestClosedCount: 6,
    pullRequestMergedCount: 5,
    pullRequestApprovedCount: 4,
    averageMergeLeadTimeHours: 18,
    repositoryCount: 1
  },
  executiveSummary: 'Resumo de teste',
  highlights: [
    {
      title: 'Entrega consistente',
      insight: 'Ritmo estável de entregas.',
      impact: 'Previsibilidade',
      evidenceIds: ['PR-101']
    }
  ],
  risks: [
    {
      risk: 'Fila de review em crescimento',
      recommendation: 'Aumentar rotação de revisores',
      evidenceIds: ['PR-202']
    }
  ],
  evidence: [
    {
      evidenceId: 'PR-101',
      evidenceType: 'pull_request',
      repositoryFullName: 'org/repo-a',
      externalReference: '#101',
      author: 'john.doe',
      occurredAt: '2026-03-15T10:00:00Z',
      status: 'approved',
      url: 'https://github.com/org/repo-a/pull/101'
    }
  ]
}

describe('report export utils', () => {
  it('builds markdown with key sections', () => {
    const markdown = buildExecutiveReportMarkdown(reportFixture)

    expect(markdown).toContain('# Relatório Executivo')
    expect(markdown).toContain('## Métricas')
    expect(markdown).toContain('Resumo de teste')
    expect(markdown).toContain('PR-101')
  })

  it('builds csv with expected rows', () => {
    const csv = buildExecutiveReportCsv(reportFixture)

    expect(csv).toContain('"section","field","value"')
    expect(csv).toContain('"meta","id","fdb89f46-6b68-4ce3-8f45-fce95af0f887"')
    expect(csv).toContain('"metrics","commitCount","12"')
    expect(csv).toContain('"evidence","id_1","PR-101"')
  })

  it('generates export filename based on format', () => {
    const fileName = getReportExportFileName(reportFixture, 'markdown')
    expect(fileName.endsWith('.md')).toBe(true)
    expect(fileName).toContain('codeimpact-report-')
  })
})
