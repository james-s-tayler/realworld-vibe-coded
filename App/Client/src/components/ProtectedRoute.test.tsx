import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { BrowserRouter, Route, Routes } from 'react-router'
import { ProtectedRoute } from './ProtectedRoute'
import { AuthProvider } from '../context/AuthContext'
import { authApi } from '../api/auth'

vi.mock('../api/auth', () => ({
  authApi: {
    getCurrentUser: vi.fn(),
  },
}))

function ProtectedContent() {
  return <div>Protected Content</div>
}

function LoginPage() {
  return <div>Login Page</div>
}

function renderProtectedRoute() {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/protected"
            element={
              <ProtectedRoute>
                <ProtectedContent />
              </ProtectedRoute>
            }
          />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

describe('ProtectedRoute', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('handles authentication check correctly', async () => {
    const mockUser = {
      email: 'test@example.com',
      username: 'testuser',
      bio: 'Test bio',
      image: null,
      token: 'test-token',
    }

    localStorage.setItem('token', 'test-token')
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    window.history.pushState({}, '', '/protected')
    renderProtectedRoute()

    // Initially should show loading, then protected content
    await waitFor(() => {
      expect(screen.getByText('Protected Content')).toBeInTheDocument()
    })
  })

  it('redirects to login when user is not authenticated', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))

    window.history.pushState({}, '', '/protected')
    renderProtectedRoute()

    await waitFor(() => {
      expect(screen.getByText('Login Page')).toBeInTheDocument()
      expect(screen.queryByText('Protected Content')).not.toBeInTheDocument()
    })
  })

  it('renders protected content when user is authenticated', async () => {
    const mockUser = {
      email: 'test@example.com',
      username: 'testuser',
      bio: 'Test bio',
      image: null,
      token: 'test-token',
    }

    localStorage.setItem('token', 'test-token')
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    window.history.pushState({}, '', '/protected')
    renderProtectedRoute()

    await waitFor(() => {
      expect(screen.getByText('Protected Content')).toBeInTheDocument()
      expect(screen.queryByText('Login Page')).not.toBeInTheDocument()
    })
  })
})
