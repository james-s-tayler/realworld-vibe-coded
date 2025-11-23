import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router'
import { SettingsPage } from './SettingsPage'
import { AuthProvider } from '../context/AuthContext'
import { authApi } from '../api/auth'

vi.mock('../api/auth', () => ({
  authApi: {
    getCurrentUser: vi.fn(),
    updateUser: vi.fn(),
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

const mockUser = {
  email: 'test@example.com',
  username: 'testuser',
  bio: 'Test bio',
  image: 'https://example.com/avatar.jpg',
  token: 'test-token',
}

function renderSettingsPage() {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <SettingsPage />
      </AuthProvider>
    </BrowserRouter>
  )
}

describe('SettingsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    localStorage.setItem('token', 'test-token')
  })

  it('renders settings form with user data', async () => {
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    renderSettingsPage()

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: /your settings/i })).toBeInTheDocument()
      expect(screen.getByDisplayValue('testuser')).toBeInTheDocument()
      expect(screen.getByDisplayValue('test@example.com')).toBeInTheDocument()
      expect(screen.getByDisplayValue('Test bio')).toBeInTheDocument()
    })
  })

  it('updates user profile successfully', async () => {
    const user = userEvent.setup()
    const updatedUser = { ...mockUser, bio: 'Completely new bio text' }
    
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    vi.mocked(authApi.updateUser).mockResolvedValue({ user: updatedUser })

    renderSettingsPage()

    await waitFor(() => {
      expect(screen.getByDisplayValue('Test bio')).toBeInTheDocument()
    })

    const bioField = screen.getByPlaceholderText(/short bio about you/i)
    await user.clear(bioField)
    await user.type(bioField, 'Completely new bio text')
    await user.click(screen.getByRole('button', { name: /update settings/i }))

    await waitFor(() => {
      expect(authApi.updateUser).toHaveBeenCalledWith({
        bio: 'Completely new bio text',
      })
      expect(screen.getByText(/settings updated successfully/i)).toBeInTheDocument()
    })
  })

  it('displays error message on update failure', async () => {
    const user = userEvent.setup()
    
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    vi.mocked(authApi.updateUser).mockRejectedValue({
      status: 422,
      errors: ['username has already been taken'],
    })

    renderSettingsPage()

    await waitFor(() => {
      expect(screen.getByDisplayValue('testuser')).toBeInTheDocument()
    })

    const usernameField = screen.getByPlaceholderText(/username/i)
    await user.clear(usernameField)
    await user.type(usernameField, 'existinguser')
    await user.click(screen.getByRole('button', { name: /update settings/i }))

    await waitFor(() => {
      expect(screen.getByText(/update failed/i)).toBeInTheDocument()
    })
  })

  it('allows updating multiple fields at once', async () => {
    const user = userEvent.setup()
    const updatedUser = {
      ...mockUser,
      username: 'newusername',
      bio: 'New bio',
      image: 'https://example.com/new-avatar.jpg',
    }
    
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    vi.mocked(authApi.updateUser).mockResolvedValue({ user: updatedUser })

    renderSettingsPage()

    await waitFor(() => {
      expect(screen.getByDisplayValue('testuser')).toBeInTheDocument()
    })

    await user.clear(screen.getByPlaceholderText(/username/i))
    await user.type(screen.getByPlaceholderText(/username/i), 'newusername')
    
    await user.clear(screen.getByPlaceholderText(/short bio about you/i))
    await user.type(screen.getByPlaceholderText(/short bio about you/i), 'New bio')
    
    await user.clear(screen.getByPlaceholderText(/url of profile picture/i))
    await user.type(screen.getByPlaceholderText(/url of profile picture/i), 'https://example.com/new-avatar.jpg')

    await user.click(screen.getByRole('button', { name: /update settings/i }))

    await waitFor(() => {
      expect(authApi.updateUser).toHaveBeenCalledWith({
        username: 'newusername',
        bio: 'New bio',
        image: 'https://example.com/new-avatar.jpg',
      })
    })
  })

  it('handles logout correctly', async () => {
    const user = userEvent.setup()
    
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    renderSettingsPage()

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /click here to logout/i })).toBeInTheDocument()
    })

    await user.click(screen.getByRole('button', { name: /click here to logout/i }))

    await waitFor(() => {
      expect(localStorage.getItem('token')).toBeNull()
      expect(mockNavigate).toHaveBeenCalledWith('/')
    })
  })

  it('allows updating password', async () => {
    const user = userEvent.setup()
    const updatedUser = { ...mockUser }
    
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    vi.mocked(authApi.updateUser).mockResolvedValue({ user: updatedUser })

    renderSettingsPage()

    await waitFor(() => {
      expect(screen.getByPlaceholderText(/new password/i)).toBeInTheDocument()
    })

    await user.type(screen.getByPlaceholderText(/new password/i), 'newpassword123')
    await user.click(screen.getByRole('button', { name: /update settings/i }))

    await waitFor(() => {
      expect(authApi.updateUser).toHaveBeenCalledWith({
        password: 'newpassword123',
      })
    })
  })
})
