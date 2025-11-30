import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router';
import {
  Form,
  TextInput,
  TextArea,
  Button,
  InlineNotification,
  Stack,
} from '@carbon/react';
import { useAuth } from '../hooks/useAuth';
import { useApiCall } from '../hooks/useApiCall';
import { PageShell } from '../components/PageShell';
import { RequestBoundary } from '../components/RequestBoundary';
import './SettingsPage.css';

export const SettingsPage: React.FC = () => {
  const navigate = useNavigate();
  const { user, updateUser, logout } = useAuth();
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [bio, setBio] = useState('');
  const [image, setImage] = useState('');
  const [password, setPassword] = useState('');
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (user) {
      setEmail(user.email);
      setUsername(user.username);
      setBio(user.bio || '');
      setImage(user.image || '');
    }
  }, [user]);

  const updateApi = useCallback(() => {
    const updates: {
      email?: string;
      username?: string;
      bio?: string;
      image?: string;
      password?: string;
    } = {};

    if (email !== user?.email) updates.email = email;
    if (username !== user?.username) updates.username = username;
    if (bio !== (user?.bio || '')) updates.bio = bio;
    if (image !== (user?.image || '')) updates.image = image;
    if (password) updates.password = password;

    return updateUser(updates);
  }, [email, username, bio, image, password, user, updateUser]);

  const { error, loading, execute, clearError } = useApiCall(updateApi, {
    onSuccess: () => {
      setSuccess(true);
      setPassword('');
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSuccess(false);
    await execute();
  };

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  if (!user) {
    return null;
  }

  return (
    <PageShell className="settings-page" title="Your Settings">
      <RequestBoundary error={error} clearError={clearError}>
        {success && (
          <InlineNotification
            kind="success"
            title="Success"
            subtitle="Settings updated successfully"
            onCloseButtonClick={() => setSuccess(false)}
            style={{ marginBottom: '1rem' }}
          />
        )}

        <Form onSubmit={handleSubmit}>
          <Stack gap={6}>
            <TextInput
              id="image"
              labelText=""
              placeholder="URL of profile picture"
              value={image}
              onChange={(e) => setImage(e.target.value)}
            />

            <TextInput
              id="username"
              labelText=""
              placeholder="Username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />

            <TextArea
              id="bio"
              labelText=""
              placeholder="Short bio about you"
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              rows={8}
            />

            <TextInput
              id="email"
              labelText=""
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              type="email"
            />

            <TextInput
              id="password"
              labelText=""
              placeholder="New Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              type="password"
            />

            <Button type="submit" disabled={loading} size="lg" className="pull-xs-right">
              {loading ? 'Updating...' : 'Update Settings'}
            </Button>
          </Stack>
        </Form>

        <hr />

        <Button kind="danger--ghost" onClick={handleLogout}>
          Or click here to logout.
        </Button>
      </RequestBoundary>
    </PageShell>
  );
};
