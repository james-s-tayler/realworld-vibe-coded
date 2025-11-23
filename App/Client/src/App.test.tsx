import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import App from './App'
import { articlesApi } from './api/articles'
import { tagsApi } from './api/tags'

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

const mockArticles = {
  articles: [],
  articlesCount: 0,
}

const mockTags = {
  tags: [],
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
    render(<App />)
    await waitFor(() => {
      expect(screen.getByRole('banner')).toBeInTheDocument()
    })
  })

  it('renders the homepage when no route is specified', async () => {
    render(<App />)
    await waitFor(() => {
      expect(screen.getByRole('heading', { name: /conduit/i })).toBeInTheDocument()
      expect(screen.getByText(/a place to share your/i)).toBeInTheDocument()
    })
  })

  it('shows sign in and sign up buttons when not authenticated', async () => {
    render(<App />)
    await waitFor(() => {
      expect(screen.getByRole('link', { name: /sign in/i })).toBeInTheDocument()
      expect(screen.getByRole('link', { name: /sign up/i })).toBeInTheDocument()
    })
  })
})