import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getAdminAuditLogs } from '../services/admin'

export default function AdminAuditPage() {
  const [action, setAction] = useState('')
  const [targetType, setTargetType] = useState('')

  const auditQuery = useQuery({
    queryKey: ['admin-audit', action, targetType],
    queryFn: () => getAdminAuditLogs({ action: action || undefined, targetType: targetType || undefined, page: 1, pageSize: 50 })
  })

  const logs = auditQuery.data?.items ?? []

  return (
    <div className="space-y-6">
      <div className="card">
        <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Backoffice</p>
        <h1 className="mt-1 text-3xl font-semibold text-on-surface">Audit logs</h1>
        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <label className="text-xs text-on-surface-variant">
            Action
            <input className="mt-1.5 w-full px-3 py-2" value={action} onChange={event => setAction(event.target.value)} />
          </label>
          <label className="text-xs text-on-surface-variant">
            Target type
            <input className="mt-1.5 w-full px-3 py-2" value={targetType} onChange={event => setTargetType(event.target.value)} />
          </label>
        </div>
      </div>

      <div className="card">
        {auditQuery.isLoading && <p className="text-sm text-on-surface-variant">Carregando auditoria...</p>}
        {auditQuery.isError && <p className="text-sm text-error">Não foi possível carregar os logs.</p>}

        {!auditQuery.isLoading && !auditQuery.isError && (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead>
                <tr className="text-xs text-on-surface-variant">
                  <th className="px-3 py-2 font-medium">Quando</th>
                  <th className="px-3 py-2 font-medium">Admin</th>
                  <th className="px-3 py-2 font-medium">Ação</th>
                  <th className="px-3 py-2 font-medium">Alvo</th>
                  <th className="px-3 py-2 font-medium">Resultado</th>
                  <th className="px-3 py-2 font-medium">Resumo</th>
                </tr>
              </thead>
              <tbody>
                {logs.map(log => (
                  <tr key={log.id} className="border-t border-outline-variant/20">
                    <td className="px-3 py-2 text-on-surface-variant">{new Date(log.createdAt).toLocaleString()}</td>
                    <td className="px-3 py-2 text-on-surface-variant text-xs font-mono">{log.adminUserId}</td>
                    <td className="px-3 py-2 text-on-surface">{log.action}</td>
                    <td className="px-3 py-2 text-on-surface-variant">{log.targetType} {log.targetId ? `#${log.targetId}` : ''}</td>
                    <td className="px-3 py-2 text-on-surface">{log.result}</td>
                    <td className="px-3 py-2 text-on-surface-variant text-xs">{log.payloadSummary}</td>
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