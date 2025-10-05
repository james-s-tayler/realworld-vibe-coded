import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router'
import { ProfilePage } from './ProfilePage'
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

function renderProfilePage() {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <ProfilePage />
      </AuthProvider>
    </BrowserRouter>
  )
}

describe('ProfilePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    localStorage.setItem('token', 'test-token')
  })

  it('renders profile form with user data', async () => {
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    renderProfilePage()

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: /your profile/i })).toBeInTheDocument()
      expect(screen.getByDisplayValue('testuser')).toBeInTheDocument()
      expect(screen.getByDisplayValue('test@example.com')).toBeInTheDocument()
      expect(screen.getByDisplayValue('Test bio')).toBeInTheDocument()
    })
  })

  it('displays current user information', async () => {
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })

    renderProfilePage()

    await waitFor(() => {
      expect(screen.getByText(/current user:/i)).toBeInTheDocument()
      expect(screen.getByText('testuser')).toBeInTheDocument()
      expect(screen.getByText(/email:/i)).toBeInTheDocument()
      expect(screen.getByText('test@example.com')).toBeInTheDocument()
    })
  })

  it('updates user profile successfully', async () => {
    const user = userEvent.setup()
    const updatedUser = { ...mockUser, bio: 'Completely new bio text' }
    
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    vi.mocked(authApi.updateUser).mockResolvedValue({ user: updatedUser })

    renderProfilePage()

    await waitFor(() => {
      expect(screen.getByDisplayValue('Test bio')).toBeInTheDocument()
    })

    const bioField = screen.getByLabelText(/bio/i)
    await user.clear(bioField)
    await user.type(bioField, 'Completely new bio text')
    await user.click(screen.getByRole('button', { name: /update profile/i }))

    await waitFor(() => {
      expect(authApi.updateUser).toHaveBeenCalledWith({
        bio: 'Completely new bio text',
      })
      expect(screen.getByText('Profile updated successfully')).toBeInTheDocument()
    })
  })

  it('displays error message on update failure', async () => {
    const user = userEvent.setup()
    
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    vi.mocked(authApi.updateUser).mockRejectedValue({
      status: 422,
      errors: ['username has already been taken'],
    })

    renderProfilePage()

    await waitFor(() => {
      expect(screen.getByDisplayValue('testuser')).toBeInTheDocument()
    })

    const usernameField = screen.getByLabelText(/username/i)
    await user.clear(usernameField)
    await user.type(usernameField, 'existinguser')
    await user.click(screen.getByRole('button', { name: /update profile/i }))

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

    renderProfilePage()

    await waitFor(() => {
      expect(screen.getByDisplayValue('testuser')).toBeInTheDocument()
    })

    await user.clear(screen.getByLabelText(/username/i))
    await user.type(screen.getByLabelText(/username/i), 'newusername')
    
    await user.clear(screen.getByLabelText(/bio/i))
    await user.type(screen.getByLabelText(/bio/i), 'New bio')
    
    await user.clear(screen.getByLabelText(/profile image url/i))
    await user.type(screen.getByLabelText(/profile image url/i), 'https://example.com/new-avatar.jpg')

    await user.click(screen.getByRole('button', { name: /update profile/i }))

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

    renderProfilePage()

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /logout/i })).toBeInTheDocument()
    })

    await user.click(screen.getByRole('button', { name: /logout/i }))

    await waitFor(() => {
      expect(localStorage.getItem('token')).toBeNull()
      expect(mockNavigate).toHaveBeenCalledWith('/login')
    })
  })

  it('allows updating password', async () => {
    const user = userEvent.setup()
    const updatedUser = { ...mockUser }
    
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    vi.mocked(authApi.updateUser).mockResolvedValue({ user: updatedUser })

    renderProfilePage()

    await waitFor(() => {
      expect(screen.getByLabelText(/new password/i)).toBeInTheDocument()
    })

    await user.type(screen.getByLabelText(/new password/i), 'newpassword123')
    await user.click(screen.getByRole('button', { name: /update profile/i }))

    await waitFor(() => {
      expect(authApi.updateUser).toHaveBeenCalledWith({
        password: 'newpassword123',
      })
    })
  })
})
