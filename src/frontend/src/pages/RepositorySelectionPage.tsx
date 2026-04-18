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
    <div className="max-w-4xl mx-auto p-6 bg-white rounded-lg shadow mt-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Repositórios GitHub</h1>
          <p className="mt-1 text-sm text-gray-600">Selecione um repositório e dispare a sincronização manual.</p>
        </div>
        <button onClick={() => navigate('/dashboard')} className="text-blue-600">Voltar ao dashboard</button>
      </div>

      {loading && <p className="mt-6">Carregando repositórios...</p>}
      {error && <p className="mt-6 text-red-600">{error}</p>}
      {success && <p className="mt-6 text-green-700">{success}</p>}

      {!loading && !error && repositories.length === 0 && (
        <p className="mt-6 text-gray-700">Nenhum repositório encontrado para esta conta GitHub.</p>
      )}

      <div className="mt-6 space-y-4">
        {repositories.map(repo => (
          <div key={repo.id} className="rounded border border-gray-200 p-4">
            <div className="flex items-center justify-between gap-4">
              <div className="flex items-start gap-3">
                <input
                  type="checkbox"
                  className="mt-1 h-4 w-4"
                  checked={Boolean(selectedIds[repo.id])}
                  onChange={() => toggleSelected(repo.id)}
                  aria-label={`Selecionar ${repo.fullName}`}
                />
                <div>
                  <p className="text-lg font-semibold">{repo.fullName}</p>
                  <p className="text-sm text-gray-500">{repo.private ? 'Privado' : 'Público'}</p>
                </div>
              </div>
              <button
                className="rounded bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-700"
                onClick={() => handleSync(repo.id)}
                disabled={syncingId === repo.id || !selectedIds[repo.id]}
              >
                {syncingId === repo.id ? 'Sincronizando...' : 'Sincronizar'}
              </button>
            </div>
          </div>
        ))}
      </div>

      <div className="mt-6 flex justify-end">
        <button
          className="rounded bg-slate-800 px-4 py-2 text-white hover:bg-slate-900 disabled:opacity-60"
          onClick={handleSaveSelection}
          disabled={savingSelection}
        >
          {savingSelection ? 'Salvando seleção...' : 'Salvar seleção'}
        </button>
      </div>
    </div>
  )
}
