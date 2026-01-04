import React, { useState, useEffect } from 'react';
import { Link } from 'react-router';
import {
  DataTable,
  TableContainer,
  Table,
  TableHead,
  TableRow,
  TableHeader,
  TableBody,
  TableCell,
  Button,
  Modal,
  TextInput,
  InlineNotification,
  Loading,
} from '@carbon/react';
import { Add } from '@carbon/icons-react';
import { PageShell } from '../components/PageShell';
import { usersApi, type User } from '../api/users';
import { ApiError } from '../api/client';
import './UsersPage.css';

const headers = [
  { key: 'username', header: 'Username' },
  { key: 'email', header: 'Email' },
  { key: 'roles', header: 'Role(s)' },
];

export const UsersPage: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [inviteModalOpen, setInviteModalOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [invitePassword, setInvitePassword] = useState('');
  const [inviteError, setInviteError] = useState<string | null>(null);
  const [inviting, setInviting] = useState(false);

  const loadUsers = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await usersApi.listUsers();
      setUsers(response.users);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.join(', '));
      } else {
        setError('Failed to load users');
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const handleInviteUser = async () => {
    if (!inviteEmail || !invitePassword) {
      setInviteError('Email and password are required');
      return;
    }

    setInviting(true);
    setInviteError(null);
    try {
      await usersApi.inviteUser(inviteEmail, invitePassword);
      setInviteModalOpen(false);
      setInviteEmail('');
      setInvitePassword('');
      await loadUsers(); // Reload the users list
    } catch (err) {
      if (err instanceof ApiError) {
        setInviteError(err.errors.join(', '));
      } else {
        setInviteError('Failed to invite user');
      }
    } finally {
      setInviting(false);
    }
  };

  const rows = users.map((user) => ({
    id: user.email,
    username: user.username,
    email: user.email,
    roles: user.roles?.join(', ') || '',
  }));

  return (
    <PageShell className="users-page">
      <div className="users-page-header">
        <h1>Users</h1>
        <Button
          renderIcon={Add}
          onClick={() => setInviteModalOpen(true)}
        >
          Invite User
        </Button>
      </div>

      {error && (
        <InlineNotification
          kind="error"
          title="Error"
          subtitle={error}
          onClose={() => setError(null)}
        />
      )}

      {loading ? (
        <Loading description="Loading users..." withOverlay={false} />
      ) : (
        <DataTable rows={rows} headers={headers}>
          {({ rows, headers, getTableProps, getHeaderProps, getRowProps }) => (
            <TableContainer>
              <Table {...getTableProps()}>
                <TableHead>
                  <TableRow>
                    {headers.map((header) => (
                      <TableHeader {...getHeaderProps({ header })} key={header.key}>
                        {header.header}
                      </TableHeader>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {rows.map((row) => (
                    <TableRow {...getRowProps({ row })} key={row.id}>
                      {row.cells.map((cell) => {
                        if (cell.info.header === 'username') {
                          return (
                            <TableCell key={cell.id}>
                              <Link to={`/profile/${cell.value}`}>{cell.value}</Link>
                            </TableCell>
                          );
                        }
                        return <TableCell key={cell.id}>{cell.value}</TableCell>;
                      })}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </DataTable>
      )}

      <Modal
        open={inviteModalOpen}
        onRequestClose={() => {
          setInviteModalOpen(false);
          setInviteEmail('');
          setInvitePassword('');
          setInviteError(null);
        }}
        modalHeading="Invite User"
        primaryButtonText="Invite"
        secondaryButtonText="Cancel"
        onRequestSubmit={handleInviteUser}
        primaryButtonDisabled={inviting}
      >
        {inviteError && (
          <InlineNotification
            kind="error"
            title="Error"
            subtitle={inviteError}
            onClose={() => setInviteError(null)}
            style={{ marginBottom: '1rem' }}
          />
        )}
        <TextInput
          id="invite-email"
          labelText="Email"
          placeholder="user@example.com"
          value={inviteEmail}
          onChange={(e) => setInviteEmail(e.target.value)}
          disabled={inviting}
        />
        <TextInput
          id="invite-password"
          labelText="Password"
          type="password"
          placeholder="Password"
          value={invitePassword}
          onChange={(e) => setInvitePassword(e.target.value)}
          disabled={inviting}
          style={{ marginTop: '1rem' }}
        />
      </Modal>
    </PageShell>
  );
};
