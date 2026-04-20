import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getAdminUserDetail, getAdminUserSubscription, updateAdminUserSubscription } from '../services/admin'

export default function AdminUserDetailPage() {
  const { id } = useParams()
  const userId = id ?? ''
  const queryClient = useQueryClient()

  const userQuery = useQuery({
    queryKey: ['admin-user-detail', userId],
    queryFn: () => getAdminUserDetail(userId),
    enabled: !!userId
  })

  const subscriptionQuery = useQuery({
    queryKey: ['admin-user-subscription', userId],
    queryFn: () => getAdminUserSubscription(userId),
    enabled: !!userId
  })

  const [saving, setSaving] = useState(false)
  const [selectedPlanId, setSelectedPlanId] = useState('')
  const [status, setStatus] = useState('active')
  const [autoRenew, setAutoRenew] = useState(true)
  const [billingIssue, setBillingIssue] = useState('')

  const handleSave = async () => {
    if (!selectedPlanId) {
      return
    }

    setSaving(true)
    try {
      await updateAdminUserSubscription(userId, {
        planId: selectedPlanId,
        status,
        autoRenew,
        currentPeriodEnd: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        billingIssue: billingIssue || undefined
      })

      await queryClient.invalidateQueries({ queryKey: ['admin-user-subscription', userId] })
    } finally {
      setSaving(false)
    }
  }

  const detail = userQuery.data
  const subscription = subscriptionQuery.data

  return (
    <div className="space-y-6">
      <div className="card">
        <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Backoffice</p>
        <h1 className="mt-1 text-3xl font-semibold text-on-surface">Detalhe do usuário</h1>
        {userQuery.isLoading && <p className="mt-3 text-sm text-on-surface-variant">Carregando dados do usuário...</p>}
        {detail && (
          <div className="mt-4 grid gap-3 md:grid-cols-3">
            <div className="metric-card"><p className="text-xs text-on-surface-variant">Email</p><p className="mt-1 text-sm text-on-surface">{detail.user.email}</p></div>
            <div className="metric-card"><p className="text-xs text-on-surface-variant">Status</p><p className="mt-1 text-sm text-on-surface">{detail.user.accountStatus}</p></div>
            <div className="metric-card"><p className="text-xs text-on-surface-variant">Último sync</p><p className="mt-1 text-sm text-on-surface">{detail.lastSyncAt ? new Date(detail.lastSyncAt).toLocaleString() : '-'}</p></div>
          </div>
        )}
      </div>

      <div className="card">
        <h2 className="text-xl font-semibold text-on-surface">Assinatura</h2>
        {subscriptionQuery.isLoading && <p className="mt-2 text-sm text-on-surface-variant">Carregando assinatura...</p>}
        {subscription && (
          <div className="mt-4 grid gap-3 md:grid-cols-2">
            <label className="text-xs text-on-surface-variant">
              Plano
              <select className="mt-1.5 w-full px-3 py-2" value={selectedPlanId || subscription.planId || ''} onChange={event => setSelectedPlanId(event.target.value)}>
                <option value="">Selecione</option>
                {subscription.availablePlans.map(plan => (
                  <option key={plan.planId} value={plan.planId}>{plan.name}</option>
                ))}
              </select>
            </label>

            <label className="text-xs text-on-surface-variant">
              Status
              <select className="mt-1.5 w-full px-3 py-2" value={status} onChange={event => setStatus(event.target.value)}>
                <option value="trial">trial</option>
                <option value="active">active</option>
                <option value="past_due">past_due</option>
                <option value="canceled">canceled</option>
              </select>
            </label>

            <label className="flex items-center gap-2 text-xs text-on-surface-variant">
              <input type="checkbox" checked={autoRenew} onChange={event => setAutoRenew(event.target.checked)} />
              Renovação automática
            </label>

            <label className="text-xs text-on-surface-variant">
              Billing issue
              <input className="mt-1.5 w-full px-3 py-2" value={billingIssue} onChange={event => setBillingIssue(event.target.value)} placeholder="Opcional" />
            </label>

            <div>
              <button type="button" className="btn-primary" onClick={handleSave} disabled={saving}>Salvar assinatura</button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}