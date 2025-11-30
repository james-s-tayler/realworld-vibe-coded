import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router';
import { ArticlePage } from './ArticlePage';
import { AuthContext } from '../context/AuthContext';
import { ToastProvider } from '../context/ToastContext';
import { articlesApi } from '../api/articles';
import { commentsApi } from '../api/comments';

// Mock the API modules
vi.mock('../api/articles', () => ({
  articlesApi: {
    getArticle: vi.fn(),
    deleteArticle: vi.fn(),
    favoriteArticle: vi.fn(),
    unfavoriteArticle: vi.fn(),
  },
}));

vi.mock('../api/comments', () => ({
  commentsApi: {
    getComments: vi.fn(),
    createComment: vi.fn(),
    deleteComment: vi.fn(),
  },
}));

vi.mock('../api/profiles', () => ({
  profilesApi: {
    followUser: vi.fn(),
    unfollowUser: vi.fn(),
  },
}));

const mockArticle = {
  slug: 'test-article',
  title: 'Test Article',
  description: 'Test description',
  body: 'Test body content',
  tagList: ['tag1', 'tag2'],
  createdAt: '2023-01-01T00:00:00Z',
  updatedAt: '2023-01-01T00:00:00Z',
  favorited: false,
  favoritesCount: 5,
  author: {
    username: 'testauthor',
    bio: 'Test bio',
    image: 'https://example.com/image.jpg',
    following: false,
  },
};

const renderWithAuth = (user = null) => {
  return render(
    <AuthContext.Provider value={{ 
      user, 
      loading: false,
      login: vi.fn(), 
      register: vi.fn(),
      logout: vi.fn(), 
      updateUser: vi.fn()
    }}>
      <ToastProvider>
        <MemoryRouter initialEntries={['/article/test-article']}>
          <Routes>
            <Route path="/article/:slug" element={<ArticlePage />} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </AuthContext.Provider>
  );
};

describe('ArticlePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(articlesApi.getArticle).mockResolvedValue({ article: mockArticle });
    vi.mocked(commentsApi.getComments).mockResolvedValue({ comments: [] });
  });

  it('renders loading state initially', () => {
    renderWithAuth();
    expect(screen.getByText(/Loading/i)).toBeInTheDocument();
  });

  it('renders article title after loading', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('Test Article')).toBeInTheDocument();
    });
  });

  it('renders author username', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getAllByText('testauthor').length).toBeGreaterThan(0);
    });
  });

  it('renders article body', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('Test body content')).toBeInTheDocument();
    });
  });

  it('renders article tags', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('tag1')).toBeInTheDocument();
      expect(screen.getByText('tag2')).toBeInTheDocument();
    });
  });
});
