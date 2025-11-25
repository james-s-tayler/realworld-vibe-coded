import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router';
import { EditorPage } from './EditorPage';
import { AuthContext } from '../context/AuthContext';

// Mock the articles API
vi.mock('../api/articles', () => ({
  getArticle: vi.fn(),
  createArticle: vi.fn(),
  updateArticle: vi.fn(),
}));

const mockUser = {
  username: 'testuser',
  email: 'test@example.com',
  token: 'test-token',
  bio: 'Test bio',
  image: 'https://example.com/image.jpg',
};

const renderWithAuth = (user = mockUser, initialRoute = '/editor') => {
  return render(
    <AuthContext.Provider value={{ 
      user, 
      loading: false,
      login: vi.fn(), 
      register: vi.fn(),
      logout: vi.fn(), 
      updateUser: vi.fn()
    }}>
      <MemoryRouter initialEntries={[initialRoute]}>
        <Routes>
          <Route path="/editor" element={<EditorPage />} />
          <Route path="/editor/:slug" element={<EditorPage />} />
        </Routes>
      </MemoryRouter>
    </AuthContext.Provider>
  );
};

describe('EditorPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders New Article heading for new articles', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByText('New Article')).toBeInTheDocument();
    });
  });

  it('renders article form fields', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByPlaceholderText('Article Title')).toBeInTheDocument();
      expect(screen.getByPlaceholderText("What's this article about?")).toBeInTheDocument();
      expect(screen.getByPlaceholderText('Write your article (in markdown)')).toBeInTheDocument();
    });
  });

  it('renders Publish Article button', async () => {
    renderWithAuth();
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Publish Article/i })).toBeInTheDocument();
    });
  });
});
