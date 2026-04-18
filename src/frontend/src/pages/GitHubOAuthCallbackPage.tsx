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
    return <div className="p-6">Finalizando o vinculo com GitHub...</div>
  }

  return (
    <div className="max-w-3xl mx-auto p-6 bg-white rounded shadow mt-8">
      <h1 className="text-2xl font-bold">Retorno do GitHub</h1>
      {error ? <p className="mt-4 text-red-600">{error}</p> : <p className="mt-4">Seu GitHub foi vinculado com sucesso.</p>}
    </div>
  )
}
