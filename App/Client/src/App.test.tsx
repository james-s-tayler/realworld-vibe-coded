import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import App from './App'
import { articlesApi } from './api/articles'
import { tagsApi } from './api/tags'
import { authApi } from './api/auth'

vi.mock('./api/articles', () => ({
  articlesApi: {
    listArticles: vi.fn(),
    getFeed: vi.fn(),
    favoriteArticle: vi.fn(),
    unfavoriteArticle: vi.fn(),
  },
}))

vi.mock('./api/tags', () => ({
  tagsApi: {
    getTags: vi.fn(),
  },
}))

vi.mock('./api/auth', () => ({
  authApi: {
    getCurrentUser: vi.fn(),
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    updateUser: vi.fn(),
  },
}))

const mockArticles = {
  articles: [],
  articlesCount: 0,
}

const mockTags = {
  tags: [],
}

const mockUser = {
  email: 'test@example.com',
  username: 'testuser',
  bio: 'Test bio',
  image: null,
}

describe('App', () => {
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear()
    vi.clearAllMocks()
    vi.mocked(articlesApi.listArticles).mockResolvedValue(mockArticles)
    vi.mocked(articlesApi.getFeed).mockResolvedValue(mockArticles)
    vi.mocked(tagsApi.getTags).mockResolvedValue(mockTags)
  })

  it('renders the app without crashing', async () => {
    // Mock unauthenticated state
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('Not authenticated'))
    render(<App />)
    await waitFor(() => {
      expect(screen.getByRole('banner')).toBeInTheDocument()
    })
  })

  it('renders and loads authenticated user', async () => {
    // Mock authenticated state
    vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
    render(<App />)
    
    // Wait for auth to finish loading
    await waitFor(() => {
      expect(vi.mocked(authApi.getCurrentUser)).toHaveBeenCalled()
    })
    
    // After auth loads, we should either see home page or be able to interact with the app
    await waitFor(() => {
      // Check that loading is done by seeing if we have the header navigation
      const navigation = screen.getByRole('navigation', { name: /main navigation/i })
      expect(navigation).toBeInTheDocument()
    }, { timeout: 3000 })
  })

  it('shows sign in and sign up buttons when not authenticated', async () => {
    // Mock unauthenticated state
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('Not authenticated'))
    render(<App />)
    await waitFor(() => {
      expect(screen.getByRole('link', { name: /sign in/i })).toBeInTheDocument()
      expect(screen.getByRole('link', { name: /sign up/i })).toBeInTheDocument()
    })
  })
})