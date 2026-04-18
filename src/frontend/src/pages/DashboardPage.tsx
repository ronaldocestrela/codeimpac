import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { getMe } from '../services/auth'

export default function DashboardPage() {
  const user = useAuthStore(state => state.user)
  const logout = useAuthStore(state => state.logout)
  const [message, setMessage] = useState('Carregando...')
  const navigate = useNavigate()

  useEffect(() => {
    if (!user) {
      getMe()
        .then(response => {
          if (response.email) {
            setMessage(`Bem-vindo, ${response.email}`)
          }
        })
        .catch(() => {
          logout()
          navigate('/login')
        })
    }
  }, [user, logout, navigate])

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="max-w-3xl mx-auto p-6 bg-white rounded-lg shadow mt-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <button
          onClick={handleLogout}
          className="rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700"
        >
          Sair
        </button>
      </div>
      <p className="mt-4 text-gray-700">{message}</p>
      {user && (
        <div className="mt-6 rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600">Usuário autenticado:</p>
          <p className="mt-2 font-semibold">{user.email}</p>
          <p className="text-sm text-gray-500">ID: {user.subject}</p>
        </div>
      )}
    </div>
  )
}
