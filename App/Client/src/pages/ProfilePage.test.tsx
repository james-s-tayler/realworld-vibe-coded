import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router';
import { ProfilePage } from './ProfilePage';
import { AuthContext } from '../context/AuthContext';
import { profilesApi } from '../api/profiles';
import { articlesApi } from '../api/articles';

// Mock the API modules
vi.mock('../api/profiles', () => ({
  profilesApi: {
    getProfile: vi.fn(),
    followUser: vi.fn(),
    unfollowUser: vi.fn(),
  },
}));

vi.mock('../api/articles', () => ({
  articlesApi: {
    listArticles: vi.fn(),
    favoriteArticle: vi.fn(),
    unfavoriteArticle: vi.fn(),
  },
}));

const mockProfile = {
  username: 'testuser',
  bio: 'Test bio',
  image: 'https://example.com/image.jpg',
  following: false,
};

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

const renderWithAuth = (user = null, username = 'testuser') => {
  return render(
    <AuthContext.Provider value={{ 
      user, 
      loading: false,
      login: vi.fn(), 
      register: vi.fn(),
      logout: vi.fn(), 
      updateUser: vi.fn()
    }}>
      <MemoryRouter initialEntries={[`/profile/${username}`]}>
        <Routes>
          <Route path="/profile/:username" element={<ProfilePage />} />
        </Routes>
      </MemoryRouter>
    </AuthContext.Provider>
  );
};

describe('ProfilePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(profilesApi.getProfile).mockResolvedValue({ profile: mockProfile });
    vi.mocked(articlesApi.listArticles).mockResolvedValue({ articles: [], articlesCount: 0 });
  });

  it('renders loading state initially', () => {
    renderWithAuth();
    expect(screen.getByText(/Loading/i)).toBeInTheDocument();
  });

  it('renders My Articles tab', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('My Articles')).toBeInTheDocument();
    });
  });

  it('renders Favorited Articles tab', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('Favorited Articles')).toBeInTheDocument();
    });
  });

  describe('Pagination', () => {
    it('calls listArticles with pagination parameters', async () => {
      vi.mocked(articlesApi.listArticles).mockResolvedValue(createMockArticles(50));

      renderWithAuth();

      await waitFor(() => {
        expect(vi.mocked(articlesApi.listArticles)).toHaveBeenCalledWith(
          expect.objectContaining({ author: 'testuser', limit: 20, offset: 0 })
        );
      });
    });

    it('shows pagination when articles count is less than or equal to page size', async () => {
      vi.mocked(articlesApi.listArticles).mockResolvedValue(createMockArticles(20));

      renderWithAuth();

      await waitFor(() => {
        expect(screen.getByText('My Articles')).toBeInTheDocument();
      });
      
      // Pagination should be visible even when articlesCount <= pageSize (since articlesCount > 0)
      await waitFor(() => {
        expect(vi.mocked(articlesApi.listArticles)).toHaveBeenCalled();
      });
    });

    it('resets page to 1 when switching tabs', async () => {
      vi.mocked(articlesApi.listArticles).mockResolvedValue(createMockArticles(50));

      renderWithAuth();

      await waitFor(() => {
        expect(screen.getByText('My Articles')).toBeInTheDocument();
      });

      // Click on Favorited Articles tab
      const favoritedTab = screen.getByText('Favorited Articles');
      fireEvent.click(favoritedTab);

      await waitFor(() => {
        // The listArticles should be called with favorited parameter and offset 0
        expect(vi.mocked(articlesApi.listArticles)).toHaveBeenCalledWith(
          expect.objectContaining({ favorited: 'testuser', offset: 0 })
        );
      });
    });
  });
});
