import { FormEvent, useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { login, getMe } from '../services/auth'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const setAuthTokens = useAuthStore(state => state.setAuthTokens)
  const setUser = useAuthStore(state => state.setUser)
  const navigate = useNavigate()

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    setLoading(true)

    try {
      const auth = await login({ email, password })
      setAuthTokens(auth)
      const user = await getMe()
      setUser(user)
      navigate('/dashboard')
    } catch (err) {
      setError('Falha no login. Verifique suas credenciais.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-[70vh] items-center justify-center">
      <div className="w-full max-w-sm">
        <div className="mb-8">
          <p className="text-xs font-semibold tracking-widest uppercase text-on-surface-variant">Acesso</p>
          <h1 className="mt-2 text-3xl font-semibold text-on-surface">Entrar na conta</h1>
        </div>
        <div className="card">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="field-label">Email</label>
              <input
                type="email"
                value={email}
                onChange={e => setEmail(e.target.value)}
                className="mt-1.5 block w-full px-3 py-2"
                placeholder="you@example.com"
                required
              />
            </div>
            <div>
              <label className="field-label">Senha</label>
              <input
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                className="mt-1.5 block w-full px-3 py-2"
                placeholder="••••••••"
                required
              />
            </div>
            {error && (
              <p className="text-xs text-error bg-error/10 px-3 py-2 rounded-md">{error}</p>
            )}
            <button
              type="submit"
              disabled={loading}
              className="btn-primary w-full"
            >
              {loading ? 'Entrando...' : 'Entrar'}
            </button>
          </form>
          <p className="mt-5 text-center text-xs text-on-surface-variant">
            Ainda não tem conta?{' '}
            <Link to="/register" className="text-primary hover:underline">Cadastre-se</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
