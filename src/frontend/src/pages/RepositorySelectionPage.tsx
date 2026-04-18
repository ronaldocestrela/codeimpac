import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  getGitHubRepositories,
  getSelectedGitHubRepositories,
  syncGitHubRepository,
  updateSelectedGitHubRepositories
} from '../services/github'
import type { GitHubRepository } from '../types/github'

export default function RepositorySelectionPage() {
  const [repositories, setRepositories] = useState<GitHubRepository[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [syncingId, setSyncingId] = useState<number | null>(null)
  const [savingSelection, setSavingSelection] = useState(false)
  const [selectedIds, setSelectedIds] = useState<Record<number, boolean>>({})
  const navigate = useNavigate()

  useEffect(() => {
    Promise.all([getGitHubRepositories(), getSelectedGitHubRepositories()])
      .then(([available, selected]) => {
        setRepositories(available)
        const selectedMap: Record<number, boolean> = {}
        selected.forEach(repo => {
          selectedMap[repo.id] = true
        })
        setSelectedIds(selectedMap)
      })
      .catch(() => setError('Não foi possível carregar os repositórios GitHub.'))
      .finally(() => setLoading(false))
  }, [])

  const toggleSelected = (id: number) => {
    setSelectedIds(prev => ({
      ...prev,
      [id]: !prev[id]
    }))
  }

  const handleSaveSelection = async () => {
    setSavingSelection(true)
    setError(null)
    setSuccess(null)

    try {
      const selectedRepositories = repositories
        .filter(repo => selectedIds[repo.id])
        .map(repo => ({
          id: repo.id,
          name: repo.name,
          fullName: repo.fullName,
          private: repo.private
        }))

      await updateSelectedGitHubRepositories({ repositories: selectedRepositories })
      setSuccess('Seleção de repositórios salva com sucesso.')
    } catch {
      setError('Não foi possível salvar a seleção de repositórios.')
    } finally {
      setSavingSelection(false)
    }
  }

  const handleSync = async (id: number) => {
    if (!selectedIds[id]) {
      setError('Selecione o repositório antes de sincronizar.')
      return
    }

    setSyncingId(id)
    setError(null)
    setSuccess(null)
    try {
      await syncGitHubRepository(id)
      setSuccess('Sincronização iniciada com sucesso.')
    } catch {
      setError('O disparo de sincronização falhou. Tente novamente.')
    } finally {
      setSyncingId(null)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">GitHub</p>
          <h1 className="mt-1 text-3xl font-semibold text-on-surface">Repositórios</h1>
          <p className="mt-1 text-sm text-on-surface-variant">Selecione repositórios e dispare a sincronização manual.</p>
        </div>
        <button onClick={() => navigate('/dashboard')} className="btn-ghost text-xs">← Dashboard</button>
      </div>

      {loading && <p className="text-sm text-on-surface-variant">Carregando repositórios...</p>}
      {error && <p className="text-sm text-error bg-error/10 px-3 py-2 rounded-md">{error}</p>}
      {success && <p className="text-sm text-success bg-success/10 px-3 py-2 rounded-md">{success}</p>}

      {!loading && !error && repositories.length === 0 && (
        <p className="text-sm text-on-surface-variant">Nenhum repositório encontrado para esta conta GitHub.</p>
      )}

      <div className="space-y-2">
        {repositories.map(repo => (
          <div key={repo.id} className="card flex items-center justify-between gap-4">
            <div className="flex items-start gap-3">
              <input
                type="checkbox"
                className="mt-1 h-4 w-4 accent-primary"
                checked={Boolean(selectedIds[repo.id])}
                onChange={() => toggleSelected(repo.id)}
                aria-label={`Selecionar ${repo.fullName}`}
              />
              <div>
                <p className="font-semibold text-on-surface font-mono text-sm">{repo.fullName}</p>
                <p className="mt-0.5 text-xs text-on-surface-variant">
                  <span className={repo.private ? 'chip-warning' : 'chip-neutral'}>
                    {repo.private ? 'Privado' : 'Público'}
                  </span>
                </p>
              </div>
            </div>
            <button
              className="btn-secondary text-xs"
              onClick={() => handleSync(repo.id)}
              disabled={syncingId === repo.id || !selectedIds[repo.id]}
            >
              {syncingId === repo.id ? 'Sincronizando...' : 'Sincronizar'}
            </button>
          </div>
        ))}
      </div>

      {repositories.length > 0 && (
        <div className="flex justify-end">
          <button
            className="btn-primary"
            onClick={handleSaveSelection}
            disabled={savingSelection}
          >
            {savingSelection ? 'Salvando...' : 'Salvar seleção'}
          </button>
        </div>
      )}
    </div>
  )
}
