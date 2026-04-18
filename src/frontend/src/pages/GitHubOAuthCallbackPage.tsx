import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { linkGitHubAccount } from '../services/github'
import type { GitHubAccount } from '../types/github'

export default function GitHubOAuthCallbackPage() {
  const [searchParams] = useSearchParams()
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const setGitHubAccount = useAuthStore(state => state.setGitHubAccount)
  const navigate = useNavigate()

  useEffect(() => {
    const code = searchParams.get('code')
    if (!code) {
      setError('Código OAuth ausente na resposta do GitHub.')
      setLoading(false)
      return
    }

    linkGitHubAccount(code)
      .then((account: GitHubAccount) => {
        setGitHubAccount(account)
        navigate('/github/repositories')
      })
      .catch(() => {
        setError('Falha ao vincular conta GitHub. Verifique se o código ainda é válido.')
      })
      .finally(() => setLoading(false))
  }, [searchParams, setGitHubAccount, navigate])

  if (loading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <p className="text-sm text-on-surface-variant">Finalizando a vinculação com GitHub...</p>
      </div>
    )
  }

  return (
    <div className="flex min-h-[50vh] items-center justify-center">
      <div className="card w-full max-w-md text-center">
        <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">GitHub OAuth</p>
        <h1 className="mt-2 text-2xl font-semibold text-on-surface">Retorno do GitHub</h1>
        {error
          ? <p className="mt-4 text-sm text-error bg-error/10 px-3 py-2 rounded-md">{error}</p>
          : <p className="mt-4 text-sm text-success">Sua conta GitHub foi vinculada com sucesso.</p>
        }
      </div>
    </div>
  )
}
