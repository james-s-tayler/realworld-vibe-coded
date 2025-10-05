import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { BrowserRouter } from 'react-router'
import { HomePage } from './HomePage'
import { AuthProvider } from '../context/AuthContext'
import { authApi } from '../api/auth'

vi.mock('../api/auth', () => ({
  authApi: {
    getCurrentUser: vi.fn(),
  },
}))

function renderHomePage() {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <HomePage />
      </AuthProvider>
    </BrowserRouter>
  )
}

describe('HomePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('renders welcome message for unauthenticated users', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))
    
    renderHomePage()

    await waitFor(() => {
      expect(screen.getByText('Welcome to Conduit')).toBeInTheDocument()
      expect(screen.getByText('A place to share your knowledge.')).toBeInTheDocument()
    })
  })

  it('shows sign in and sign up links for unauthenticated users', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))
    
    renderHomePage()

    await waitFor(() => {
      expect(screen.getByRole('link', { name: /sign in/i })).toHaveAttribute('href', '/login')
      expect(screen.getByRole('link', { name: /sign up/i })).toHaveAttribute('href', '/register')
    })
  })

  it('shows personalized greeting for authenticated users', async () => {
    const mockUser = {
      email: 'test@example.com',
      username: 'testuser',
      bio: 'Test bio',
      image: null,
      token: 'test-token',
    }

    localStorage.setItem('token', 'test-token')
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    renderHomePage()

    await waitFor(() => {
      expect(screen.getByText(/welcome back, testuser/i)).toBeInTheDocument()
      expect(screen.getByText(/you are logged in to conduit/i)).toBeInTheDocument()
    })
  })

  it('shows profile link for authenticated users', async () => {
    const mockUser = {
      email: 'test@example.com',
      username: 'testuser',
      bio: 'Test bio',
      image: null,
      token: 'test-token',
    }

    localStorage.setItem('token', 'test-token')
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    renderHomePage()

    await waitFor(() => {
      expect(screen.getByRole('link', { name: /view profile/i })).toHaveAttribute('href', '/profile')
    })
  })
})
