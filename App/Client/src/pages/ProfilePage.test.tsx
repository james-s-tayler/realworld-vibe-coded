import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router';
import ProfilePage from './ProfilePage';
import { AuthContext } from '../context/AuthContext';

// Mock the API modules
vi.mock('../api/profiles', () => ({
  getProfile: vi.fn().mockResolvedValue({
    profile: {
      username: 'testuser',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      following: false,
    }
  }),
  followUser: vi.fn(),
  unfollowUser: vi.fn(),
}));

vi.mock('../api/articles', () => ({
  listArticles: vi.fn().mockResolvedValue({
    articles: [],
    articlesCount: 0,
  }),
  favoriteArticle: vi.fn(),
  unfavoriteArticle: vi.fn(),
}));

const renderWithAuth = (user = null, username = 'testuser') => {
  return render(
    <AuthContext.Provider value={{ 
      user, 
      token: user?.token || null, 
      login: vi.fn(), 
      logout: vi.fn(), 
      isLoading: false 
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
  });

  it('renders loading state initially', () => {
    renderWithAuth();
    expect(screen.getByText(/Loading/i)).toBeInTheDocument();
  });

  it('renders My Posts tab', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('My Posts')).toBeInTheDocument();
    });
  });

  it('renders Favorited Posts tab', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('Favorited Posts')).toBeInTheDocument();
    });
  });
});
