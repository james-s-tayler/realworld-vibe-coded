import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router'
import { LoginPage } from './LoginPage'
import { AuthProvider } from '../context/AuthContext'
import { authApi } from '../api/auth'

vi.mock('../api/auth', () => ({
  authApi: {
    login: vi.fn(),
    getCurrentUser: vi.fn(),
  },
}))

const mockNavigate = vi.fn()
vi.mock('react-router', async () => {
  const actual = await vi.importActual('react-router')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

function renderLoginPage() {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <LoginPage />
      </AuthProvider>
    </BrowserRouter>
  )
}

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))
  })

  it('renders login form', () => {
    renderLoginPage()
    
    expect(screen.getByRole('heading', { name: /sign in/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('shows link to registration page', () => {
    renderLoginPage()
    
    expect(screen.getByText(/don't have an account/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /sign up/i })).toHaveAttribute('href', '/register')
  })

  it('submits login form with valid credentials', async () => {
    const user = userEvent.setup()
    const mockUser = {
      email: 'test@example.com',
      username: 'testuser',
      bio: '',
      image: null,
      token: 'test-token',
    }

    vi.mocked(authApi.login).mockResolvedValue({ user: mockUser })

    renderLoginPage()

    await user.type(screen.getByLabelText(/email/i), 'test@example.com')
    await user.type(screen.getByLabelText(/password/i), 'password123')
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(authApi.login).toHaveBeenCalledWith('test@example.com', 'password123')
      expect(mockNavigate).toHaveBeenCalledWith('/')
    })
  })

  it('displays error message on login failure', async () => {
    const user = userEvent.setup()
    
    vi.mocked(authApi.login).mockRejectedValue({
      status: 401,
      errors: ['email or password is invalid'],
    })

    renderLoginPage()

    await user.type(screen.getByLabelText(/email/i), 'test@example.com')
    await user.type(screen.getByLabelText(/password/i), 'wrongpassword')
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(screen.getByText(/login failed/i)).toBeInTheDocument()
    })
  })
})
