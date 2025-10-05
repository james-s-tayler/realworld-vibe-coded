import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { AuthProvider } from './AuthContext'
import { useAuth } from '../hooks/useAuth'
import { authApi } from '../api/auth'

// Mock the auth API
vi.mock('../api/auth', () => ({
  authApi: {
    login: vi.fn(),
    register: vi.fn(),
    getCurrentUser: vi.fn(),
    updateUser: vi.fn(),
  },
}))

// Test component that uses the auth context
function TestComponent() {
  const { user, loading } = useAuth()
  
  if (loading) {
    return <div>Loading...</div>
  }
  
  return (
    <div>
      {user ? (
        <div>Logged in as {user.username}</div>
      ) : (
        <div>Not logged in</div>
      )}
    </div>
  )
}

describe('AuthContext', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('provides initial unauthenticated state', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))
    
    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByText('Not logged in')).toBeInTheDocument()
    })
  })

  it('loads user from token on mount if token exists', async () => {
    const mockUser = {
      email: 'test@example.com',
      username: 'testuser',
      bio: 'Test bio',
      image: null,
      token: 'test-token',
    }

    localStorage.setItem('token', 'test-token')
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByText('Logged in as testuser')).toBeInTheDocument()
    })
  })

  it('removes token from localStorage on auth failure', async () => {
    localStorage.setItem('token', 'invalid-token')
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('Invalid token'))

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByText('Not logged in')).toBeInTheDocument()
      expect(localStorage.getItem('token')).toBeNull()
    })
  })
})
