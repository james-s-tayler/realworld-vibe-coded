import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { UsersPage } from './UsersPage';
import { usersApi } from '../api/users';

vi.mock('../api/users', () => ({
  usersApi: {
    listUsers: vi.fn(),
    inviteUser: vi.fn(),
    deactivateUser: vi.fn(),
    reactivateUser: vi.fn(),
    updateUserRoles: vi.fn(),
  },
}));

vi.mock('../hooks/useAuth', () => ({
  useAuth: () => ({
    user: { email: 'admin@test.com', username: 'admin', bio: '', image: null, roles: ['OWNER', 'ADMIN', 'USER'] },
    loading: false,
  }),
}));

const mockUser = (overrides = {}) => ({
  id: crypto.randomUUID(),
  email: 'user@test.com',
  username: 'user',
  bio: 'Bio',
  image: null,
  roles: ['USER'],
  isActive: true,
  ...overrides,
});

describe('UsersPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders users table with status column', async () => {
    const mockUsers = [
      mockUser({ id: '1', email: 'user1@test.com', username: 'user1', roles: ['USER'] }),
      mockUser({ id: '2', email: 'user2@test.com', username: 'user2', roles: ['ADMIN', 'USER'] }),
    ];

    vi.mocked(usersApi.listUsers).mockResolvedValue({ users: mockUsers, usersCount: 2 });

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('user1')).toBeInTheDocument();
      expect(screen.getByText('user2')).toBeInTheDocument();
      expect(screen.getByText('user1@test.com')).toBeInTheDocument();
      expect(screen.getByText('user2@test.com')).toBeInTheDocument();
    });
  });

  it('shows loading state while fetching users', () => {
    vi.mocked(usersApi.listUsers).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    expect(screen.getByText('Loading users...')).toBeInTheDocument();
  });

  it('displays error when fetching users fails', async () => {
    vi.mocked(usersApi.listUsers).mockRejectedValue(new Error('Failed to fetch'));

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Failed to load users')).toBeInTheDocument();
    });
  });

  it('opens invite modal when invite button is clicked', async () => {
    const user = userEvent.setup();
    vi.mocked(usersApi.listUsers).mockResolvedValue({ users: [], usersCount: 0 });

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.queryByText('Loading users...')).not.toBeInTheDocument();
    });

    const inviteButton = screen.getByRole('button', { name: /invite user/i });
    await user.click(inviteButton);

    expect(screen.getByRole('heading', { name: 'Invite User' })).toBeInTheDocument();
    expect(screen.getByLabelText('Email')).toBeInTheDocument();
    expect(screen.getByLabelText('Password')).toBeInTheDocument();
  });

  it('invites user and refreshes list', async () => {
    const user = userEvent.setup();
    const initialUsers = [
      mockUser({ id: '1', email: 'user1@test.com', username: 'user1' }),
    ];
    const updatedUsers = [
      ...initialUsers,
      mockUser({ id: '2', email: 'user2@test.com', username: 'user2' }),
    ];

    vi.mocked(usersApi.listUsers)
      .mockResolvedValueOnce({ users: initialUsers, usersCount: 1 })
      .mockResolvedValueOnce({ users: updatedUsers, usersCount: 2 });
    vi.mocked(usersApi.inviteUser).mockResolvedValue();

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('user1')).toBeInTheDocument();
    });

    // Open invite modal
    const inviteButton = screen.getByRole('button', { name: /invite user/i });
    await user.click(inviteButton);

    // Fill in form
    const emailInput = screen.getByLabelText('Email');
    const passwordInput = screen.getByLabelText('Password');
    await user.type(emailInput, 'user2@test.com');
    await user.type(passwordInput, 'password123');

    // Submit
    const submitButton = screen.getByRole('button', { name: /^invite$/i });
    await user.click(submitButton);

    // Verify invite was called
    await waitFor(() => {
      expect(usersApi.inviteUser).toHaveBeenCalledWith('user2@test.com', 'password123');
    });

    // Verify users list was refreshed
    await waitFor(() => {
      expect(screen.getByText('user2')).toBeInTheDocument();
    });
  });

  it('shows error when invite fails', async () => {
    const user = userEvent.setup();
    vi.mocked(usersApi.listUsers).mockResolvedValue({ users: [], usersCount: 0 });
    vi.mocked(usersApi.inviteUser).mockRejectedValue(new Error('Invite failed'));

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.queryByText('Loading users...')).not.toBeInTheDocument();
    });

    // Open invite modal
    const inviteButton = screen.getByRole('button', { name: /invite user/i });
    await user.click(inviteButton);

    // Fill in form
    const emailInput = screen.getByLabelText('Email');
    const passwordInput = screen.getByLabelText('Password');
    await user.type(emailInput, 'user2@test.com');
    await user.type(passwordInput, 'password123');

    // Submit
    const submitButton = screen.getByRole('button', { name: /^invite$/i });
    await user.click(submitButton);

    // Verify error is shown
    await waitFor(() => {
      expect(screen.getByText('Failed to invite user')).toBeInTheDocument();
    });
  });

  it('passes pagination params to listUsers', async () => {
    vi.mocked(usersApi.listUsers).mockResolvedValue({ users: [], usersCount: 0 });

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(usersApi.listUsers).toHaveBeenCalledWith(50, 0);
    });
  });

  it('shows active status tags for active users', async () => {
    const mockUsers = [
      mockUser({ id: '1', email: 'user1@test.com', username: 'user1', isActive: true }),
    ];

    vi.mocked(usersApi.listUsers).mockResolvedValue({ users: mockUsers, usersCount: 1 });

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Active')).toBeInTheDocument();
    });
  });

  it('shows deactivated status tag for inactive users', async () => {
    const mockUsers = [
      mockUser({ id: '1', email: 'user1@test.com', username: 'user1', isActive: false }),
    ];

    vi.mocked(usersApi.listUsers).mockResolvedValue({ users: mockUsers, usersCount: 1 });

    render(
      <MemoryRouter>
        <UsersPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Deactivated')).toBeInTheDocument();
    });
  });
});
