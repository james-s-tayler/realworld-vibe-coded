import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
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
});
