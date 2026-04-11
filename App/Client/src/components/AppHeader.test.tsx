import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { AppHeader } from './AppHeader';
import { AuthContext } from '../context/AuthContext';
import type { User } from '../types/user';

const mockLogout = vi.fn();

const renderWithAuth = (user: User | null) => {
  mockLogout.mockClear();
  return render(
    <AuthContext.Provider value={{
      user,
      loading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: mockLogout,
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

  it('renders Home link for authenticated users', () => {
    renderWithAuth({
      username: 'testuser',
      email: 'test@example.com',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      roles: ['AUTHOR']
    });
    expect(screen.getByText('Home')).toBeInTheDocument();
  });

  it('does not render Home link for unauthenticated users', () => {
    renderWithAuth(null);
    expect(screen.queryByText('Home')).not.toBeInTheDocument();
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
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      roles: ['AUTHOR']
    });
    expect(screen.getByText('New Article')).toBeInTheDocument();
    expect(screen.getByText('Settings')).toBeInTheDocument();
  });

  it('renders username link when logged in', () => {
    renderWithAuth({
      username: 'testuser',
      email: 'test@example.com',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      roles: ['AUTHOR']
    });
    expect(screen.getByText('testuser')).toBeInTheDocument();
  });

  it('renders Users link for ADMIN users', () => {
    renderWithAuth({
      username: 'admin',
      email: 'admin@example.com',
      bio: 'Admin bio',
      image: 'https://example.com/image.jpg',
      roles: ['ADMIN', 'AUTHOR']
    });
    expect(screen.getByText('Users')).toBeInTheDocument();
  });

  it('does not render Users link for non-ADMIN users', () => {
    renderWithAuth({
      username: 'testuser',
      email: 'test@example.com',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      roles: ['AUTHOR']
    });
    expect(screen.queryByText('Users')).not.toBeInTheDocument();
  });

  it('does not render Users link when user has no roles', () => {
    renderWithAuth({
      username: 'testuser',
      email: 'test@example.com',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      roles: []
    });
    expect(screen.queryByText('Users')).not.toBeInTheDocument();
  });

  it('renders Log out link for authenticated users', () => {
    renderWithAuth({
      username: 'testuser',
      email: 'test@example.com',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      roles: ['AUTHOR']
    });
    expect(screen.getByText('Log out')).toBeInTheDocument();
  });

  it('calls logout when Log out link is clicked', async () => {
    const user = userEvent.setup();
    renderWithAuth({
      username: 'testuser',
      email: 'test@example.com',
      bio: 'Test bio',
      image: 'https://example.com/image.jpg',
      roles: ['AUTHOR']
    });
    await user.click(screen.getByText('Log out'));
    expect(mockLogout).toHaveBeenCalled();
  });

  it('does not render Log out link for unauthenticated users', () => {
    renderWithAuth(null);
    expect(screen.queryByText('Log out')).not.toBeInTheDocument();
  });
});
