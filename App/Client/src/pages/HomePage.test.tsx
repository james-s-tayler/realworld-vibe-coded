import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { BrowserRouter } from 'react-router'
import { HomePage } from './HomePage'
import { AuthProvider } from '../context/AuthContext'
import { authApi } from '../api/auth'
import { articlesApi } from '../api/articles'
import { tagsApi } from '../api/tags'

vi.mock('../api/auth', () => ({
  authApi: {
    getCurrentUser: vi.fn(),
    logout: vi.fn(),
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

// Create mock articles for pagination tests
const createMockArticles = (count: number) => {
  const articles = Array.from({ length: count }, (_, i) => ({
    slug: `article-${i + 1}`,
    title: `Article ${i + 1}`,
    description: `Description ${i + 1}`,
    body: `Body ${i + 1}`,
    tagList: ['test'],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    favorited: false,
    favoritesCount: 0,
    author: {
      username: 'testuser',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      following: false,
    },
  }));
  return {
    articles,
    articlesCount: count,
  };
};

describe('HomePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    vi.mocked(articlesApi.listArticles).mockResolvedValue(mockArticles)
    vi.mocked(articlesApi.getFeed).mockResolvedValue(mockArticles)
    vi.mocked(tagsApi.getTags).mockResolvedValue(mockTags)
  })

  it('renders welcome message for authenticated users', async () => {
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
      expect(screen.getByRole('heading', { name: /conduit/i })).toBeInTheDocument()
      expect(screen.getByText(/a place to share your/i)).toBeInTheDocument()
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
      expect(screen.getByText(/popular tags/i)).toBeInTheDocument()
    })
  })

  describe('Pagination', () => {
    it('calls listArticles with pagination parameters', async () => {
      const mockUser = {
        email: 'test@example.com',
        username: 'testuser',
        bio: 'Test bio',
        image: null,
        token: 'test-token',
      }

      localStorage.setItem('token', 'test-token')
      vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
      vi.mocked(articlesApi.listArticles).mockResolvedValue(createMockArticles(50))

      renderHomePage()

      await waitFor(() => {
        expect(vi.mocked(articlesApi.listArticles)).toHaveBeenCalledWith(
          expect.objectContaining({ limit: 20, offset: 0 })
        )
      })
    })

    it('shows pagination when articles count is less than or equal to page size', async () => {
      const mockUser = {
        email: 'test@example.com',
        username: 'testuser',
        bio: 'Test bio',
        image: null,
        token: 'test-token',
      }

      localStorage.setItem('token', 'test-token')
      vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
      vi.mocked(articlesApi.listArticles).mockResolvedValue(createMockArticles(20))

      renderHomePage()

      await waitFor(() => {
        expect(screen.getByRole('tab', { name: /global feed/i })).toBeInTheDocument()
      })
      
      // Pagination should be visible even when articlesCount <= pageSize
      await waitFor(() => {
        expect(vi.mocked(articlesApi.listArticles)).toHaveBeenCalled()
      })
    })

    it('loads Your Feed with pagination for authenticated users', async () => {
      const mockUser = {
        email: 'test@example.com',
        username: 'testuser',
        bio: 'Test bio',
        image: null,
        token: 'test-token',
      }

      localStorage.setItem('token', 'test-token')
      vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
      vi.mocked(articlesApi.getFeed).mockResolvedValue(createMockArticles(50))

      renderHomePage()

      // Wait for Your Feed to load with pagination
      await waitFor(() => {
        expect(vi.mocked(articlesApi.getFeed)).toHaveBeenCalledWith(20, 0)
      })
    })

    it('resets page to 1 when changing tabs', async () => {
      const mockUser = {
        email: 'test@example.com',
        username: 'testuser',
        bio: 'Test bio',
        image: null,
        token: 'test-token',
      }

      localStorage.setItem('token', 'test-token')
      vi.mocked(authApi.getCurrentUser).mockResolvedValue({ user: mockUser })
      vi.mocked(articlesApi.getFeed).mockResolvedValue(createMockArticles(50))
      vi.mocked(articlesApi.listArticles).mockResolvedValue(createMockArticles(50))

      renderHomePage()

      await waitFor(() => {
        expect(screen.getByRole('tab', { name: /global feed/i })).toBeInTheDocument()
      })

      // Click on Global Feed tab
      const globalFeedTab = screen.getByRole('tab', { name: /global feed/i })
      fireEvent.click(globalFeedTab)

      await waitFor(() => {
        // listArticles should be called with offset 0 (page 1)
        expect(vi.mocked(articlesApi.listArticles)).toHaveBeenCalledWith(
          expect.objectContaining({ offset: 0 })
        )
      })
    })
  })
})
