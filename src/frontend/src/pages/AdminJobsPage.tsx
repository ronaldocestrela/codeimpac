import { useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getAdminJobs, retryAdminJob } from '../services/admin'

export default function AdminJobsPage() {
  const queryClient = useQueryClient()
  const [jobType, setJobType] = useState('')
  const [status, setStatus] = useState('')
  const [retryingJobId, setRetryingJobId] = useState<string | null>(null)

  const jobsQuery = useQuery({
    queryKey: ['admin-jobs', jobType, status],
    queryFn: () => getAdminJobs({ jobType: jobType || undefined, status: status || undefined, page: 1, pageSize: 50 }),
    refetchInterval: data => (data?.items.some(job => job.status === 'Queued' || job.status === 'Processing') ? 2000 : 8000)
  })

  const handleRetry = async (taskId: string) => {
    setRetryingJobId(taskId)
    try {
      await retryAdminJob(taskId)
      await queryClient.invalidateQueries({ queryKey: ['admin-jobs'] })
    } finally {
      setRetryingJobId(null)
    }
  }

  const jobs = jobsQuery.data?.items ?? []

  return (
    <div className="space-y-6">
      <div className="card">
        <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Backoffice</p>
        <h1 className="mt-1 text-3xl font-semibold text-on-surface">Jobs</h1>
        <p className="mt-1 text-sm text-on-surface-variant">Monitoramento de sync, AI summary e geração de relatórios.</p>

        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <label className="text-xs text-on-surface-variant">
            Tipo
            <select className="mt-1.5 w-full px-3 py-2" value={jobType} onChange={event => setJobType(event.target.value)}>
              <option value="">Todos</option>
              <option value="ContributionSummary">ContributionSummary</option>
              <option value="ExecutiveReport">ExecutiveReport</option>
            </select>
          </label>
          <label className="text-xs text-on-surface-variant">
            Status
            <select className="mt-1.5 w-full px-3 py-2" value={status} onChange={event => setStatus(event.target.value)}>
              <option value="">Todos</option>
              <option value="Queued">Queued</option>
              <option value="Processing">Processing</option>
              <option value="Succeeded">Succeeded</option>
              <option value="Failed">Failed</option>
            </select>
          </label>
        </div>
      </div>

      <div className="card">
        {jobsQuery.isLoading && <p className="text-sm text-on-surface-variant">Carregando jobs...</p>}
        {jobsQuery.isError && <p className="text-sm text-error">Não foi possível carregar os jobs.</p>}

        {!jobsQuery.isLoading && !jobsQuery.isError && (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead>
                <tr className="text-xs text-on-surface-variant">
                  <th className="px-3 py-2 font-medium">TaskId</th>
                  <th className="px-3 py-2 font-medium">User</th>
                  <th className="px-3 py-2 font-medium">Tipo</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 font-medium">Criado em</th>
                  <th className="px-3 py-2 font-medium">Erro</th>
                  <th className="px-3 py-2 font-medium">Ações</th>
                </tr>
              </thead>
              <tbody>
                {jobs.map(job => (
                  <tr key={job.taskId} className="border-t border-outline-variant/20">
                    <td className="px-3 py-2 text-on-surface-variant text-xs font-mono">{job.taskId}</td>
                    <td className="px-3 py-2 text-on-surface-variant text-xs font-mono">{job.userId}</td>
                    <td className="px-3 py-2 text-on-surface-variant">{job.jobType}</td>
                    <td className="px-3 py-2 text-on-surface">{job.status}</td>
                    <td className="px-3 py-2 text-on-surface-variant">{new Date(job.createdAt).toLocaleString()}</td>
                    <td className="px-3 py-2 text-tertiary text-xs">{job.errorMessage ?? '-'}</td>
                    <td className="px-3 py-2">
                      <button
                        type="button"
                        className="btn-secondary text-xs py-1.5"
                        disabled={job.status !== 'Failed' || retryingJobId === job.taskId}
                        onClick={() => handleRetry(job.taskId)}
                      >
                        Retry
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}