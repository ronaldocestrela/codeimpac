import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { forceAdminUserResync, getAdminUsers, revokeAdminUserGitHubAccess, updateAdminUserStatus } from '../services/admin'

export default function AdminUsersPage() {
  const queryClient = useQueryClient()
  const [email, setEmail] = useState('')
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const [loadingUserId, setLoadingUserId] = useState<string | null>(null)

  const usersQuery = useQuery({
    queryKey: ['admin-users', email, status, page],
    queryFn: () => getAdminUsers({ email: email || undefined, status: status || undefined, page, pageSize: 20 })
  })

  const refreshUsers = async () => {
    await queryClient.invalidateQueries({ queryKey: ['admin-users'] })
  }

  const handleSuspend = async (userId: string) => {
    setLoadingUserId(userId)
    try {
      await updateAdminUserStatus(userId, 'Suspended', 'Suspensão manual no backoffice')
      await refreshUsers()
    } finally {
      setLoadingUserId(null)
    }
  }

  const handleActivate = async (userId: string) => {
    setLoadingUserId(userId)
    try {
      await updateAdminUserStatus(userId, 'Active', 'Reativação manual no backoffice')
      await refreshUsers()
    } finally {
      setLoadingUserId(null)
    }
  }

  const handleRevokeGitHub = async (userId: string) => {
    setLoadingUserId(userId)
    try {
      await revokeAdminUserGitHubAccess(userId)
      await refreshUsers()
    } finally {
      setLoadingUserId(null)
    }
  }

  const handleResync = async (userId: string) => {
    setLoadingUserId(userId)
    try {
      await forceAdminUserResync(userId)
      await refreshUsers()
    } finally {
      setLoadingUserId(null)
    }
  }

  const users = usersQuery.data?.items ?? []

  return (
    <div className="space-y-6">
      <div className="card">
        <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Backoffice</p>
        <h1 className="mt-1 text-3xl font-semibold text-on-surface">Usuários</h1>
        <p className="mt-1 text-sm text-on-surface-variant">Gestão de contas, suporte e operações manuais.</p>

        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <label className="text-xs text-on-surface-variant">
            Email
            <input className="mt-1.5 w-full px-3 py-2" value={email} onChange={event => setEmail(event.target.value)} />
          </label>
          <label className="text-xs text-on-surface-variant">
            Status
            <select className="mt-1.5 w-full px-3 py-2" value={status} onChange={event => setStatus(event.target.value)}>
              <option value="">Todos</option>
              <option value="Active">Active</option>
              <option value="Suspended">Suspended</option>
              <option value="Blocked">Blocked</option>
            </select>
          </label>
          <div className="flex items-end">
            <button type="button" className="btn-secondary" onClick={() => setPage(1)}>Aplicar filtros</button>
          </div>
        </div>
      </div>

      <div className="card">
        {usersQuery.isLoading && <p className="text-sm text-on-surface-variant">Carregando usuários...</p>}
        {usersQuery.isError && <p className="text-sm text-error">Não foi possível carregar os usuários.</p>}

        {!usersQuery.isLoading && !usersQuery.isError && (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead>
                <tr className="text-xs text-on-surface-variant">
                  <th className="px-3 py-2 font-medium">Email</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 font-medium">Plano</th>
                  <th className="px-3 py-2 font-medium">Uso</th>
                  <th className="px-3 py-2 font-medium">Ações</th>
                </tr>
              </thead>
              <tbody>
                {users.map(user => (
                  <tr key={user.userId} className="border-t border-outline-variant/20">
                    <td className="px-3 py-2 text-on-surface">
                      <Link className="text-primary hover:underline" to={`/admin/users/${user.userId}`}>{user.email}</Link>
                    </td>
                    <td className="px-3 py-2 text-on-surface-variant">{user.accountStatus}</td>
                    <td className="px-3 py-2 text-on-surface-variant">{user.planName ?? '-'}</td>
                    <td className="px-3 py-2 text-on-surface-variant">
                      Repos: {user.repositoriesUsed} · Relatórios: {user.reportsUsedThisMonth}
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex flex-wrap gap-2">
                        <button type="button" className="btn-secondary text-xs py-1.5" disabled={loadingUserId === user.userId} onClick={() => handleSuspend(user.userId)}>Suspender</button>
                        <button type="button" className="btn-secondary text-xs py-1.5" disabled={loadingUserId === user.userId} onClick={() => handleActivate(user.userId)}>Ativar</button>
                        <button type="button" className="btn-secondary text-xs py-1.5" disabled={loadingUserId === user.userId} onClick={() => handleRevokeGitHub(user.userId)}>Revogar GitHub</button>
                        <button type="button" className="btn-secondary text-xs py-1.5" disabled={loadingUserId === user.userId} onClick={() => handleResync(user.userId)}>Forçar resync</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <div className="mt-4 flex gap-2">
          <button type="button" className="btn-secondary text-xs py-1.5" onClick={() => setPage(current => Math.max(1, current - 1))} disabled={page <= 1}>Anterior</button>
          <button type="button" className="btn-secondary text-xs py-1.5" onClick={() => setPage(current => current + 1)} disabled={(usersQuery.data?.items.length ?? 0) < 20}>Próxima</button>
        </div>
      </div>
    </div>
  )
}