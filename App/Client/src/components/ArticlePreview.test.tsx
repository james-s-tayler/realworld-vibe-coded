import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { ArticlePreview } from './ArticlePreview';
import { AuthContext } from '../context/AuthContext';

const mockArticle = {
  slug: 'test-article',
  title: 'Test Article Title',
  description: 'Test article description',
  body: 'Test article body',
  tagList: ['tag1', 'tag2'],
  createdAt: '2023-01-01T00:00:00Z',
  updatedAt: '2023-01-01T00:00:00Z',
  favorited: false,
  favoritesCount: 5,
  author: {
    username: 'testuser',
    bio: 'Test bio',
    image: 'https://example.com/image.jpg',
    following: false,
  },
};

const renderWithRouter = (ui: React.ReactElement, authValue = { user: null, loading: false, login: vi.fn(), register: vi.fn(), logout: vi.fn(), updateUser: vi.fn() }) => {
  return render(
    <AuthContext.Provider value={authValue}>
      <MemoryRouter>{ui}</MemoryRouter>
    </AuthContext.Provider>
  );
};

describe('ArticlePreview', () => {
  it('renders article title and description', () => {
    renderWithRouter(<ArticlePreview article={mockArticle} />);
    expect(screen.getByText('Test Article Title')).toBeInTheDocument();
    expect(screen.getByText('Test article description')).toBeInTheDocument();
  });

  it('renders author username', () => {
    renderWithRouter(<ArticlePreview article={mockArticle} />);
    expect(screen.getByText('testuser')).toBeInTheDocument();
  });

  it('renders favorites count', () => {
    renderWithRouter(<ArticlePreview article={mockArticle} />);
    expect(screen.getByText('5')).toBeInTheDocument();
  });

  it('renders tags', () => {
    renderWithRouter(<ArticlePreview article={mockArticle} />);
    expect(screen.getByText('tag1')).toBeInTheDocument();
    expect(screen.getByText('tag2')).toBeInTheDocument();
  });

  it('renders Read more link', () => {
    renderWithRouter(<ArticlePreview article={mockArticle} />);
    expect(screen.getByText('Read more...')).toBeInTheDocument();
  });
});
