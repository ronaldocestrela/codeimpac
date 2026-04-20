import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getAdminUsers } from '../services/admin'

export default function AdminSubscriptionsPage() {
  const usersQuery = useQuery({
    queryKey: ['admin-subscriptions'],
    queryFn: () => getAdminUsers({ page: 1, pageSize: 100 })
  })

  const users = usersQuery.data?.items ?? []

  return (
    <div className="space-y-6">
      <div className="card">
        <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Backoffice</p>
        <h1 className="mt-1 text-3xl font-semibold text-on-surface">Assinaturas</h1>
        <p className="mt-1 text-sm text-on-surface-variant">Visão operacional de plano e status de assinatura por usuário.</p>
      </div>

      <div className="card">
        {usersQuery.isLoading && <p className="text-sm text-on-surface-variant">Carregando assinaturas...</p>}
        {usersQuery.isError && <p className="text-sm text-error">Não foi possível carregar assinaturas.</p>}

        {!usersQuery.isLoading && !usersQuery.isError && (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead>
                <tr className="text-xs text-on-surface-variant">
                  <th className="px-3 py-2 font-medium">Usuário</th>
                  <th className="px-3 py-2 font-medium">Plano</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 font-medium">Uso</th>
                  <th className="px-3 py-2 font-medium">Ações</th>
                </tr>
              </thead>
              <tbody>
                {users.map(user => (
                  <tr key={user.userId} className="border-t border-outline-variant/20">
                    <td className="px-3 py-2 text-on-surface">{user.email}</td>
                    <td className="px-3 py-2 text-on-surface-variant">{user.planName ?? '-'}</td>
                    <td className="px-3 py-2 text-on-surface-variant">{user.subscriptionStatus ?? 'trial'}</td>
                    <td className="px-3 py-2 text-on-surface-variant">Repos: {user.repositoriesUsed} · Relatórios: {user.reportsUsedThisMonth}</td>
                    <td className="px-3 py-2"><Link to={`/admin/users/${user.userId}`} className="text-primary hover:underline">Gerenciar</Link></td>
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