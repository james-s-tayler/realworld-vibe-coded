import React, { useState, useEffect, useCallback } from 'react';
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
  Pagination,
  Tag,
  Checkbox,
  OverflowMenu,
  OverflowMenuItem,
} from '@carbon/react';
import { Add } from '@carbon/icons-react';
import { PageShell } from '../components/PageShell';
import { usersApi, type User } from '../api/users';
import { useAuth } from '../hooks/useAuth';
import { ApiError } from '../api/client';
import './UsersPage.css';

const DEFAULT_PAGE_SIZE = 20;
const PAGE_SIZE_OPTIONS = [10, 20, 50];
const ASSIGNABLE_ROLES = ['ADMIN'];

const headers = [
  { key: 'username', header: 'Username' },
  { key: 'email', header: 'Email' },
  { key: 'roles', header: 'Role(s)' },
  { key: 'status', header: 'Status' },
  { key: 'actions', header: 'Actions' },
];

export const UsersPage: React.FC = () => {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [usersCount, setUsersCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  // Invite modal
  const [inviteModalOpen, setInviteModalOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [invitePassword, setInvitePassword] = useState('');
  const [inviteError, setInviteError] = useState<string | null>(null);
  const [inviting, setInviting] = useState(false);

  // Edit roles modal
  const [editRolesModalOpen, setEditRolesModalOpen] = useState(false);
  const [editRolesUser, setEditRolesUser] = useState<User | null>(null);
  const [editRolesSelected, setEditRolesSelected] = useState<string[]>([]);
  const [editRolesError, setEditRolesError] = useState<string | null>(null);
  const [editRolesSaving, setEditRolesSaving] = useState(false);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const offset = (currentPage - 1) * pageSize;
      const response = await usersApi.listUsers(pageSize, offset);
      setUsers(response.users);
      setUsersCount(response.usersCount);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.join(', '));
      } else {
        setError('Failed to load users');
      }
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize]);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  const handlePageChange = ({ page, pageSize: newPageSize }: { page: number; pageSize: number }) => {
    setCurrentPage(page);
    setPageSize(newPageSize);
  };

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
      await loadUsers();
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

  const handleDeactivate = async (user: User) => {
    try {
      await usersApi.deactivateUser(user.id);
      await loadUsers();
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.join(', '));
      } else {
        setError('Failed to deactivate user');
      }
    }
  };

  const handleReactivate = async (user: User) => {
    try {
      await usersApi.reactivateUser(user.id);
      await loadUsers();
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.join(', '));
      } else {
        setError('Failed to reactivate user');
      }
    }
  };

  const openEditRolesModal = (user: User) => {
    setEditRolesUser(user);
    setEditRolesSelected(user.roles.filter((r) => ASSIGNABLE_ROLES.includes(r)));
    setEditRolesError(null);
    setEditRolesModalOpen(true);
  };

  const handleEditRolesSubmit = async () => {
    if (!editRolesUser) {
      return;
    }

    setEditRolesSaving(true);
    setEditRolesError(null);
    try {
      await usersApi.updateUserRoles(editRolesUser.id, editRolesSelected);
      setEditRolesModalOpen(false);
      setEditRolesUser(null);
      await loadUsers();
    } catch (err) {
      if (err instanceof ApiError) {
        setEditRolesError(err.errors.join(', '));
      } else {
        setEditRolesError('Failed to update roles');
      }
    } finally {
      setEditRolesSaving(false);
    }
  };

  const handleRoleToggle = (role: string, checked: boolean) => {
    if (checked) {
      setEditRolesSelected((prev) => [...prev, role]);
    } else {
      setEditRolesSelected((prev) => prev.filter((r) => r !== role));
    }
  };

  const isSelf = (user: User) => currentUser?.email === user.email;

  const rows = users.map((user) => ({
    id: user.id,
    username: user.username,
    email: user.email,
    roles: user.roles?.join(', ') || '',
    status: user.isActive ? 'Active' : 'Deactivated',
    actions: user,
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
        <>
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
                    {rows.map((row) => {
                      const userData = users.find((u) => u.id === row.id);
                      return (
                        <TableRow {...getRowProps({ row })} key={row.id}>
                          {row.cells.map((cell) => {
                            if (cell.info.header === 'username') {
                              return (
                                <TableCell key={cell.id}>
                                  <Link to={`/profile/${cell.value}`}>{cell.value}</Link>
                                </TableCell>
                              );
                            }
                            if (cell.info.header === 'status' && userData) {
                              return (
                                <TableCell key={cell.id}>
                                  <Tag type={userData.isActive ? 'green' : 'red'} size="sm">
                                    {userData.isActive ? 'Active' : 'Deactivated'}
                                  </Tag>
                                </TableCell>
                              );
                            }
                            if (cell.info.header === 'actions' && userData) {
                              const showDeactivateToggle = !isSelf(userData);
                              return (
                                <TableCell key={cell.id}>
                                  <OverflowMenu iconDescription={`Actions for ${userData.email}`} flipped>
                                    <OverflowMenuItem
                                      itemText="Edit Roles"
                                      onClick={() => openEditRolesModal(userData)}
                                    />
                                    {showDeactivateToggle && userData.isActive && (
                                      <OverflowMenuItem
                                        itemText="Deactivate"
                                        isDelete
                                        onClick={() => handleDeactivate(userData)}
                                      />
                                    )}
                                    {showDeactivateToggle && !userData.isActive && (
                                      <OverflowMenuItem
                                        itemText="Reactivate"
                                        onClick={() => handleReactivate(userData)}
                                      />
                                    )}
                                  </OverflowMenu>
                                </TableCell>
                              );
                            }
                            return <TableCell key={cell.id}>{cell.value}</TableCell>;
                          })}
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </DataTable>
          {usersCount > 0 && (
            <Pagination
              page={currentPage}
              pageSize={pageSize}
              pageSizes={PAGE_SIZE_OPTIONS}
              totalItems={usersCount}
              onChange={handlePageChange}
            />
          )}
        </>
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

      <Modal
        open={editRolesModalOpen}
        onRequestClose={() => {
          setEditRolesModalOpen(false);
          setEditRolesUser(null);
          setEditRolesError(null);
        }}
        modalHeading={`Edit Roles — ${editRolesUser?.username ?? ''}`}
        primaryButtonText="Save"
        secondaryButtonText="Cancel"
        onRequestSubmit={handleEditRolesSubmit}
        primaryButtonDisabled={editRolesSaving}
      >
        {editRolesError && (
          <InlineNotification
            kind="error"
            title="Error"
            subtitle={editRolesError}
            onClose={() => setEditRolesError(null)}
            style={{ marginBottom: '1rem' }}
          />
        )}
        <div className="edit-roles-checkboxes">
          {ASSIGNABLE_ROLES.map((role) => (
            <Checkbox
              key={role}
              id={`role-${role.toLowerCase()}`}
              labelText={role}
              checked={editRolesSelected.includes(role)}
              onChange={(_: React.ChangeEvent<HTMLInputElement>, { checked }: { checked: boolean }) => handleRoleToggle(role, checked)}
              disabled={editRolesSaving}
            />
          ))}
        </div>
      </Modal>
    </PageShell>
  );
};
