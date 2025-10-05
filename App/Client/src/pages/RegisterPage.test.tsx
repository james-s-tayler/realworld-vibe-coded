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
    localStorage.clear()
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))
  })

  it('renders registration form', () => {
    renderRegisterPage()
    
    expect(screen.getByRole('heading', { name: /sign up/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign up/i })).toBeInTheDocument()
  })

  it('shows link to login page', () => {
    renderRegisterPage()
    
    expect(screen.getByText(/already have an account/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /sign in/i })).toHaveAttribute('href', '/login')
  })

  it('submits registration form with valid data', async () => {
    const user = userEvent.setup()
    const mockUser = {
      email: 'newuser@example.com',
      username: 'newuser',
      bio: '',
      image: null,
      token: 'new-token',
    }

    vi.mocked(authApi.register).mockResolvedValue({ user: mockUser })

    renderRegisterPage()

    await user.type(screen.getByLabelText(/username/i), 'newuser')
    await user.type(screen.getByLabelText(/email/i), 'newuser@example.com')
    await user.type(screen.getByLabelText(/password/i), 'password123')
    await user.click(screen.getByRole('button', { name: /sign up/i }))

    await waitFor(() => {
      expect(authApi.register).toHaveBeenCalledWith('newuser@example.com', 'newuser', 'password123')
      expect(mockNavigate).toHaveBeenCalledWith('/')
    })
  })

  it('displays error message on registration failure', async () => {
    const user = userEvent.setup()
    
    vi.mocked(authApi.register).mockRejectedValue({
      status: 422,
      errors: ['username has already been taken'],
    })

    renderRegisterPage()

    await user.type(screen.getByLabelText(/username/i), 'existinguser')
    await user.type(screen.getByLabelText(/email/i), 'test@example.com')
    await user.type(screen.getByLabelText(/password/i), 'password123')
    await user.click(screen.getByRole('button', { name: /sign up/i }))

    await waitFor(() => {
      expect(screen.getByText(/registration failed/i)).toBeInTheDocument()
    })
  })
})
