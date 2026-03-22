import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router';
import { ProfilePage } from './ProfilePage';
import { AuthContext } from '../context/AuthContext';
import { profilesApi } from '../api/profiles';

vi.mock('../api/profiles', () => ({
  profilesApi: {
    getProfile: vi.fn(),
  },
}));

const mockProfile = {
  username: 'testuser',
  bio: 'Test bio',
  image: 'https://example.com/image.jpg',
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
  });

  it('renders loading state initially', () => {
    renderWithAuth();
    expect(screen.getByText(/Loading/i)).toBeInTheDocument();
  });

  it('renders profile info after loading', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('testuser')).toBeInTheDocument();
    });
    expect(screen.getByText('Test bio')).toBeInTheDocument();
  });

  it('shows edit profile button for own profile', async () => {
    renderWithAuth({
      username: 'testuser',
      email: 'test@example.com',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      roles: ['USER']
    });
    await waitFor(() => {
      expect(screen.getByText('Edit Profile Settings')).toBeInTheDocument();
    });
  });

  it('does not show edit profile button for other profiles', async () => {
    renderWithAuth({
      username: 'otheruser',
      email: 'other@example.com',
      bio: 'Other bio',
      image: null,
      roles: ['USER']
    });
    await waitFor(() => {
      expect(screen.getByText('testuser')).toBeInTheDocument();
    });
    expect(screen.queryByText('Edit Profile Settings')).not.toBeInTheDocument();
  });
});
