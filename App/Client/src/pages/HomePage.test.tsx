import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { BrowserRouter } from 'react-router'
import { HomePage } from './HomePage'
import { AuthProvider } from '../context/AuthContext'
import { authApi } from '../api/auth'
import { articlesApi } from '../api/articles'
import { tagsApi } from '../api/tags'

vi.mock('../api/auth', () => ({
  authApi: {
    getCurrentUser: vi.fn(),
  },
}))

vi.mock('../api/articles', () => ({
  articlesApi: {
    listArticles: vi.fn(),
    getFeed: vi.fn(),
    favoriteArticle: vi.fn(),
    unfavoriteArticle: vi.fn(),
  },
}))

vi.mock('../api/tags', () => ({
  tagsApi: {
    getTags: vi.fn(),
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

const mockArticles = {
  articles: [],
  articlesCount: 0,
}

const mockTags = {
  tags: [],
}

describe('HomePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    vi.mocked(articlesApi.listArticles).mockResolvedValue(mockArticles)
    vi.mocked(articlesApi.getFeed).mockResolvedValue(mockArticles)
    vi.mocked(tagsApi.getTags).mockResolvedValue(mockTags)
  })

  it('renders welcome message for unauthenticated users', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))
    
    renderHomePage()

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: /conduit/i })).toBeInTheDocument()
      expect(screen.getByText(/a place to share your/i)).toBeInTheDocument()
    })
  })

  it('shows Global Feed tab for unauthenticated users', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))
    
    renderHomePage()

    await waitFor(() => {
      expect(screen.getByRole('tab', { name: /global feed/i })).toBeInTheDocument()
      expect(screen.queryByRole('tab', { name: /your feed/i })).not.toBeInTheDocument()
    })
  })

  it('shows Your Feed and Global Feed tabs for authenticated users', async () => {
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
      expect(screen.getByRole('tab', { name: /your feed/i })).toBeInTheDocument()
      expect(screen.getByRole('tab', { name: /global feed/i })).toBeInTheDocument()
    })
  })

  it('shows Your Feed tab for authenticated users', async () => {
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
      expect(screen.getByRole('tab', { name: /your feed/i })).toBeInTheDocument()
    })
  })

  it('shows Popular Tags sidebar', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('No token'))
    
    renderHomePage()

    await waitFor(() => {
      expect(screen.getByText(/popular tags/i)).toBeInTheDocument()
    })
  })
})
