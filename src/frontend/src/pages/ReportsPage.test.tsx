import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ReportsPage from './ReportsPage'

const mocks = vi.hoisted(() => ({
  enqueueExecutiveReportGeneration: vi.fn(),
  getExecutiveReports: vi.fn(),
  getExecutiveReportDetail: vi.fn(),
  exportExecutiveReport: vi.fn(),
  getBackgroundJobStatus: vi.fn(),
  getSelectedGitHubRepositories: vi.fn(),
  getGitHubRepositories: vi.fn()
}))

vi.mock('../services/reports', () => ({
  enqueueExecutiveReportGeneration: mocks.enqueueExecutiveReportGeneration,
  getExecutiveReports: mocks.getExecutiveReports,
  getExecutiveReportDetail: mocks.getExecutiveReportDetail,
  exportExecutiveReport: mocks.exportExecutiveReport
}))

vi.mock('../services/backgroundJobs', () => ({
  getBackgroundJobStatus: mocks.getBackgroundJobStatus
}))

vi.mock('../services/github', () => ({
  getSelectedGitHubRepositories: mocks.getSelectedGitHubRepositories,
  getGitHubRepositories: mocks.getGitHubRepositories
}))

function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false
      },
      mutations: {
        retry: false
      }
    }
  })
}

function renderPage() {
  return render(
    <QueryClientProvider client={createQueryClient()}>
      <ReportsPage />
    </QueryClientProvider>
  )
}

function createReport(reportId: string) {
  return {
    id: reportId,
    generatedAt: '2026-04-20T12:00:00Z',
    scope: {
      developerScope: 'Developer',
      repositories: []
    },
    metrics: {
      commitCount: 0,
      pullRequestOpenCount: 0,
      pullRequestClosedCount: 0,
      pullRequestMergedCount: 0,
      pullRequestApprovedCount: 0,
      repositoryCount: 0
    },
    executiveSummary: 'Resumo',
    highlights: [],
    risks: [],
    evidence: []
  }
}

describe('ReportsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    mocks.getSelectedGitHubRepositories.mockResolvedValue([])
    mocks.getGitHubRepositories.mockResolvedValue([])
    mocks.getExecutiveReports.mockResolvedValue([])
    mocks.getBackgroundJobStatus.mockResolvedValue({
      taskId: 'task-1',
      jobType: 'ExecutiveReport',
      status: 'Succeeded',
      createdAt: '2026-04-20T12:00:00Z',
      reportId: 'report-1'
    })
    mocks.getExecutiveReportDetail.mockResolvedValue(createReport('report-1'))
    mocks.enqueueExecutiveReportGeneration.mockResolvedValue({
      taskId: 'task-1',
      jobType: 'ExecutiveReport',
      status: 'Queued',
      createdAt: '2026-04-20T12:00:00Z'
    })
  })

  it('re-enables generate button when job succeeds with reportId', async () => {
    const user = userEvent.setup()
    renderPage()

    const button = await screen.findByRole('button', { name: 'Gerar Relatório' })
    await user.click(button)

    await waitFor(() => {
      expect(mocks.getBackgroundJobStatus).toHaveBeenCalledWith('task-1')
    })

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Gerar Relatório' })).toBeEnabled()
    })

    expect(screen.getByText('Relatorio concluido com sucesso.')).toBeInTheDocument()
  })

  it('re-enables generate button when job succeeds without reportId', async () => {
    mocks.getBackgroundJobStatus.mockResolvedValue({
      taskId: 'task-1',
      jobType: 'ExecutiveReport',
      status: 'Succeeded',
      createdAt: '2026-04-20T12:00:00Z',
      reportId: null
    })

    const user = userEvent.setup()
    renderPage()

    const button = await screen.findByRole('button', { name: 'Gerar Relatório' })
    await user.click(button)

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Gerar Relatório' })).toBeEnabled()
    })

    expect(screen.getByText('Relatorio concluido, mas sem identificador para abrir automaticamente.')).toBeInTheDocument()
  })
})
