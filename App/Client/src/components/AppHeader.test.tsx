import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { AppHeader } from './AppHeader';
import { AuthContext } from '../context/AuthContext';

const renderWithAuth = (user: { username: string; email: string; token: string; bio: string; image: string } | null) => {
  return render(
    <AuthContext.Provider value={{ 
      user, 
      loading: false,
      login: vi.fn(), 
      register: vi.fn(),
      logout: vi.fn(), 
      updateUser: vi.fn()
    }}>
      <MemoryRouter>
        <AppHeader />
      </MemoryRouter>
    </AuthContext.Provider>
  );
};

describe('AppHeader', () => {
  it('renders conduit brand link', () => {
    renderWithAuth(null);
    expect(screen.getByText('conduit')).toBeInTheDocument();
  });

  it('renders Home link', () => {
    renderWithAuth(null);
    expect(screen.getByText('Home')).toBeInTheDocument();
  });

  it('renders Sign in and Sign up links when logged out', () => {
    renderWithAuth(null);
    expect(screen.getByText('Sign in')).toBeInTheDocument();
    expect(screen.getByText('Sign up')).toBeInTheDocument();
  });

  it('renders New Article and Settings when logged in', () => {
    renderWithAuth({ 
      username: 'testuser', 
      email: 'test@example.com', 
      token: 'test-token',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg'
    });
    expect(screen.getByText('New Article')).toBeInTheDocument();
    expect(screen.getByText('Settings')).toBeInTheDocument();
  });

  it('renders username link when logged in', () => {
    renderWithAuth({ 
      username: 'testuser', 
      email: 'test@example.com', 
      token: 'test-token',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg'
    });
    expect(screen.getByText('testuser')).toBeInTheDocument();
  });
});
