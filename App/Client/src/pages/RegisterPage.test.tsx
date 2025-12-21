import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router'
import { RegisterPage } from './RegisterPage'
import { AuthProvider } from '../context/AuthContext'
import { authApi } from '../api/auth'

vi.mock('../api/auth', () => ({
  authApi: {
    register: vi.fn(),
    login: vi.fn(),
    logout: vi.fn(),
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

function renderRegisterPage() {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <RegisterPage />
      </AuthProvider>
    </BrowserRouter>
  )
}

describe('RegisterPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No session'))
  })

  it('renders registration form', () => {
    renderRegisterPage()
    
    expect(screen.getByRole('heading', { name: /sign up/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign up/i })).toBeInTheDocument()
  })

  it('shows link to login page', () => {
    renderRegisterPage()
    
    expect(screen.getByText(/have an account/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /have an account/i })).toHaveAttribute('href', '/login')
  })

  it('submits registration form with valid data', async () => {
    const user = userEvent.setup()
    const mockUser = {
      email: 'newuser@example.com',
      username: 'newuser@example.com',
      bio: '',
      image: null,
      token: 'new-token',
    }

    vi.mocked(authApi.register).mockResolvedValue(undefined)
    vi.mocked(authApi.login).mockResolvedValue()
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    renderRegisterPage()

    await user.type(screen.getByLabelText(/email/i), 'newuser@example.com')
    await user.type(screen.getByLabelText(/password/i), 'password123')
    await user.click(screen.getByRole('button', { name: /sign up/i }))

    await waitFor(() => {
      expect(authApi.register).toHaveBeenCalledWith('newuser@example.com', 'password123')
      expect(authApi.login).toHaveBeenCalledWith('newuser@example.com', 'password123')
      expect(mockNavigate).toHaveBeenCalledWith('/')
    })
  })

  it('displays error message on registration failure', async () => {
    const user = userEvent.setup()
    
    // Throw an error that normalizeError can handle
    vi.mocked(authApi.register).mockRejectedValue(new Error('email has already been taken'))

    renderRegisterPage()

    await user.type(screen.getByLabelText(/email/i), 'test@example.com')
    await user.type(screen.getByLabelText(/password/i), 'password123')
    await user.click(screen.getByRole('button', { name: /sign up/i }))

    await waitFor(() => {
      expect(screen.getByText(/email has already been taken/i)).toBeInTheDocument()
    })
  })
})
