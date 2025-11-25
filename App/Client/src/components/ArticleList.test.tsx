import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { ArticleList } from './ArticleList';
import { AuthContext } from '../context/AuthContext';

const mockArticles = [
  {
    slug: 'test-article-1',
    title: 'Test Article 1',
    description: 'Description 1',
    body: 'Body 1',
    tagList: ['tag1'],
    createdAt: '2023-01-01T00:00:00Z',
    updatedAt: '2023-01-01T00:00:00Z',
    favorited: false,
    favoritesCount: 5,
    author: {
      username: 'user1',
      bio: 'Bio 1',
      image: 'https://example.com/image1.jpg',
      following: false,
    },
  },
  {
    slug: 'test-article-2',
    title: 'Test Article 2',
    description: 'Description 2',
    body: 'Body 2',
    tagList: ['tag2'],
    createdAt: '2023-01-02T00:00:00Z',
    updatedAt: '2023-01-02T00:00:00Z',
    favorited: true,
    favoritesCount: 10,
    author: {
      username: 'user2',
      bio: 'Bio 2',
      image: 'https://example.com/image2.jpg',
      following: true,
    },
  },
];

const renderWithRouter = (ui: React.ReactElement) => {
  return render(
    <AuthContext.Provider value={{ user: null, loading: false, login: vi.fn(), register: vi.fn(), logout: vi.fn(), updateUser: vi.fn() }}>
      <MemoryRouter>{ui}</MemoryRouter>
    </AuthContext.Provider>
  );
};

describe('ArticleList', () => {
  it('renders loading state', () => {
    renderWithRouter(
      <ArticleList articles={[]} loading={true} />
    );
    expect(screen.getByText('Loading articles...')).toBeInTheDocument();
  });

  it('renders empty state when no articles', () => {
    renderWithRouter(
      <ArticleList articles={[]} loading={false} />
    );
    expect(screen.getByText('No articles are here... yet.')).toBeInTheDocument();
  });

  it('renders articles when provided', () => {
    renderWithRouter(
      <ArticleList articles={mockArticles} loading={false} />
    );
    expect(screen.getByText('Test Article 1')).toBeInTheDocument();
    expect(screen.getByText('Test Article 2')).toBeInTheDocument();
  });

  it('renders multiple article descriptions', () => {
    renderWithRouter(
      <ArticleList articles={mockArticles} loading={false} />
    );
    expect(screen.getByText('Description 1')).toBeInTheDocument();
    expect(screen.getByText('Description 2')).toBeInTheDocument();
  });
});
