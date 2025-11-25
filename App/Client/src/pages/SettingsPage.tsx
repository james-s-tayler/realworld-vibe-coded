import React, { useState, useEffect } from 'react';
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
import { ApiError } from '../api/client';
import './SettingsPage.css';

export const SettingsPage: React.FC = () => {
  const navigate = useNavigate();
  const { user, updateUser, logout } = useAuth();
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [bio, setBio] = useState('');
  const [image, setImage] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (user) {
      setEmail(user.email);
      setUsername(user.username);
      setBio(user.bio || '');
      setImage(user.image || '');
    }
  }, [user]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(false);
    setLoading(true);

    try {
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

      await updateUser(updates);
      setSuccess(true);
      setPassword('');
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.join(', '));
      } else {
        setError('An unexpected error occurred');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  if (!user) {
    return null;
  }

  return (
    <div className="settings-page">
      <div className="container page">
        <div className="row">
          <div className="col-md-6 offset-md-3 col-xs-12">
            <h1 className="text-xs-center">Your Settings</h1>

            {error && (
              <InlineNotification
                kind="error"
                title="Update Failed"
                subtitle={error}
                onCloseButtonClick={() => setError(null)}
                style={{ marginBottom: '1rem' }}
              />
            )}

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
          </div>
        </div>
      </div>
    </div>
  );
};
