import { render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { articlesApi } from './api/articles'
import { authApi } from './api/auth'
import { tagsApi } from './api/tags'
import App from './App'

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

vi.mock('./api/featureFlagsApi', () => ({
  featureFlagsApi: {
    getConfig: vi.fn().mockResolvedValue({
      feature_management: { feature_flags: [] },
    }),
  },
}))

vi.mock('./api/configApi', () => ({
  configApi: {
    getConfig: vi.fn().mockResolvedValue({
      featureFlagRefreshIntervalSeconds: 60,
    }),
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
    localStorage.clear()
    vi.clearAllMocks()
    vi.mocked(articlesApi.listArticles).mockResolvedValue(mockArticles)
    vi.mocked(articlesApi.getFeed).mockResolvedValue(mockArticles)
    vi.mocked(tagsApi.getTags).mockResolvedValue(mockTags)
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
      const navigation = screen.getByRole('navigation', { name: /side navigation/i })
      expect(navigation).toBeInTheDocument()
    }, { timeout: 3000 })
  })

  it('shows sign in and sign up buttons when not authenticated', async () => {
    vi.mocked(authApi.getCurrentUser).mockRejectedValue(new Error('Not authenticated'))
    render(<App />)
    await waitFor(() => {
      expect(screen.getByRole('link', { name: /sign in/i })).toBeInTheDocument()
      expect(screen.getByRole('link', { name: /sign up/i })).toBeInTheDocument()
    })
  })
})
