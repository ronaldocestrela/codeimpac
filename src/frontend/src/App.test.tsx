import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import App from './App'

describe('App', () => {
  it('renders the home page and login links', () => {
    render(
      <MemoryRouter initialEntries={['/']}>
        <App />
      </MemoryRouter>
    )

    expect(screen.getByText('CodeImpact Frontend')).toBeInTheDocument()
    expect(screen.getAllByRole('link', { name: /login/i })).toHaveLength(2)
    expect(screen.getByRole('link', { name: /cadastro/i })).toBeInTheDocument()
  })

  it('renders the login page when navigating to /login', () => {
    render(
      <MemoryRouter initialEntries={['/login']}>
        <App />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: /login/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /entrar/i })).toBeInTheDocument()
  })
})
