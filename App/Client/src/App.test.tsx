import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import App from './App'
import { authApi } from './api/auth'

vi.mock('./api/auth', () => ({
  authApi: {
    getCurrentUser: vi.fn(),
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    updateUser: vi.fn(),
  },
}))

const mockUser = {
  email: 'test@example.com',
  username: 'testuser',
  bio: 'Test bio',
  image: null,
}

describe('App', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('renders the app without crashing', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('Not authenticated'))
    render(<App />)
    await waitFor(() => {
      expect(screen.getByRole('banner')).toBeInTheDocument()
    })
  })

  it('renders and loads authenticated user', async () => {
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    render(<App />)

    await waitFor(() => {
      expect(vi.mocked(authApi.getCurrentUser)).toHaveBeenCalled()
    })

    await waitFor(() => {
      const navigations = screen.getAllByRole('navigation', { name: /side navigation/i })
      expect(navigations.length).toBeGreaterThan(0)
    }, { timeout: 3000 })
  })

  it('shows sign in and sign up buttons when not authenticated', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('Not authenticated'))
    render(<App />)
    await waitFor(() => {
      expect(screen.getAllByRole('link', { name: /sign in/i }).length).toBeGreaterThan(0)
      expect(screen.getAllByRole('link', { name: /sign up/i }).length).toBeGreaterThan(0)
    })
  })
})
