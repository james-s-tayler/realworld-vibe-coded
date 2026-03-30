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
import { useTranslation } from 'react-i18next';
import { PageShell } from '../components/PageShell';
import { usersApi, type User } from '../api/users';
import { useAuth } from '../hooks/useAuth';
import { ApiError } from '../api/client';
import './UsersPage.css';

const DEFAULT_PAGE_SIZE = 20;
const PAGE_SIZE_OPTIONS = [10, 20, 50];
const ASSIGNABLE_ROLES = ['ADMIN'];

export const UsersPage: React.FC = () => {
  const { t } = useTranslation();
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
        setError(t('users.failedToLoad'));
      }
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize, t]);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  const handlePageChange = ({ page, pageSize: newPageSize }: { page: number; pageSize: number }) => {
    setCurrentPage(page);
    setPageSize(newPageSize);
  };

  const handleInviteUser = async () => {
    if (!inviteEmail || !invitePassword) {
      setInviteError(t('users.emailAndPasswordRequired'));
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
        setInviteError(t('users.failedToInvite'));
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
        setError(t('users.failedToDeactivate'));
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
        setError(t('users.failedToReactivate'));
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
        setEditRolesError(t('users.failedToUpdateRoles'));
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

  const headers = [
    { key: 'username', header: t('users.username') },
    { key: 'email', header: t('users.email') },
    { key: 'roles', header: t('users.roles') },
    { key: 'status', header: t('users.status') },
    { key: 'actions', header: t('users.actions') },
  ];

  const rows = users.map((user) => ({
    id: user.id,
    username: user.username,
    email: user.email,
    roles: user.roles?.join(', ') || '',
    status: user.isActive ? t('users.active') : t('users.deactivated'),
    actions: user,
  }));

  return (
    <PageShell className="users-page">
      <div className="users-page-header">
        <h1>{t('users.title')}</h1>
        <Button
          renderIcon={Add}
          onClick={() => setInviteModalOpen(true)}
        >
          {t('users.inviteUser')}
        </Button>
      </div>

      {error && (
        <InlineNotification
          kind="error"
          title={t('error.title')}
          subtitle={error}
          onClose={() => setError(null)}
        />
      )}

      {loading ? (
        <Loading description={t('users.loading')} withOverlay={false} />
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
                                    {userData.isActive ? t('users.active') : t('users.deactivated')}
                                  </Tag>
                                </TableCell>
                              );
                            }
                            if (cell.info.header === 'actions' && userData) {
                              const showDeactivateToggle = !isSelf(userData);
                              return (
                                <TableCell key={cell.id}>
                                  <OverflowMenu iconDescription={t('users.actionsFor', { email: userData.email })} flipped>
                                    <OverflowMenuItem
                                      itemText={t('users.editRoles')}
                                      onClick={() => openEditRolesModal(userData)}
                                    />
                                    {showDeactivateToggle && userData.isActive && (
                                      <OverflowMenuItem
                                        itemText={t('users.deactivate')}
                                        isDelete
                                        onClick={() => handleDeactivate(userData)}
                                      />
                                    )}
                                    {showDeactivateToggle && !userData.isActive && (
                                      <OverflowMenuItem
                                        itemText={t('users.reactivate')}
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
              backwardText={t('pagination.previous')}
              forwardText={t('pagination.next')}
              itemsPerPageText={t('pagination.itemsPerPage')}
              itemRangeText={(min, max, total) => t('pagination.itemRange', { min, max, total })}
              pageRangeText={(_current, total) => t('pagination.pageRange', { count: total })}
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
        modalHeading={t('users.inviteModalTitle')}
        primaryButtonText={t('users.invite')}
        secondaryButtonText={t('users.cancel')}
        onRequestSubmit={handleInviteUser}
        primaryButtonDisabled={inviting}
      >
        {inviteError && (
          <InlineNotification
            kind="error"
            title={t('error.title')}
            subtitle={inviteError}
            onClose={() => setInviteError(null)}
            style={{ marginBottom: '1rem' }}
          />
        )}
        <TextInput
          id="invite-email"
          labelText={t('users.email')}
          placeholder={t('users.emailPlaceholder')}
          value={inviteEmail}
          onChange={(e) => setInviteEmail(e.target.value)}
          disabled={inviting}
        />
        <TextInput
          id="invite-password"
          labelText={t('users.password')}
          type="password"
          placeholder={t('users.password')}
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
        modalHeading={t('users.editRolesTitle', { username: editRolesUser?.username ?? '' })}
        primaryButtonText={t('users.save')}
        secondaryButtonText={t('users.cancel')}
        onRequestSubmit={handleEditRolesSubmit}
        primaryButtonDisabled={editRolesSaving}
      >
        {editRolesError && (
          <InlineNotification
            kind="error"
            title={t('error.title')}
            subtitle={editRolesError}
            onClose={() => setEditRolesError(null)}
            style={{ marginBottom: '1rem' }}
          />
        )}
        <div className="edit-roles-checkboxes">
          {editRolesUser?.roles.includes('OWNER') && (
            <Checkbox
              id="role-owner"
              labelText="OWNER"
              checked
              disabled
            />
          )}
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
