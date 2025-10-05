import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import {
  Form,
  TextInput,
  TextArea,
  Button,
  InlineNotification,
  Stack,
  Tile,
} from '@carbon/react';
import { useAuth } from '../hooks/useAuth';
import { ApiError } from '../api/client';

export const ProfilePage: React.FC = () => {
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
    navigate('/login');
  };

  if (!user) {
    return null;
  }

  return (
    <div style={{ maxWidth: '600px', margin: '2rem auto', padding: '0 1rem' }}>
      <h1 style={{ marginBottom: '2rem' }}>Your Profile</h1>
      
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
          subtitle="Profile updated successfully"
          onCloseButtonClick={() => setSuccess(false)}
          style={{ marginBottom: '1rem' }}
        />
      )}

      <Tile style={{ marginBottom: '2rem', padding: '1rem' }}>
        <p><strong>Current User:</strong> {user.username}</p>
        <p><strong>Email:</strong> {user.email}</p>
      </Tile>

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
          <TextInput
            id="image"
            labelText="Profile Image URL"
            placeholder="Enter URL of profile picture"
            value={image}
            onChange={(e) => setImage(e.target.value)}
          />

          <TextInput
            id="username"
            labelText="Username"
            placeholder="Enter your username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
          
          <TextArea
            id="bio"
            labelText="Bio"
            placeholder="Tell us about yourself"
            value={bio}
            onChange={(e) => setBio(e.target.value)}
            rows={4}
          />
          
          <TextInput
            id="email"
            labelText="Email"
            placeholder="Enter your email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            type="email"
          />
          
          <TextInput
            id="password"
            labelText="New Password"
            placeholder="Leave blank to keep current password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            type="password"
          />

          <div style={{ display: 'flex', gap: '1rem' }}>
            <Button type="submit" disabled={loading}>
              {loading ? 'Updating...' : 'Update Profile'}
            </Button>
            
            <Button kind="danger" onClick={handleLogout}>
              Logout
            </Button>
          </div>
        </Stack>
      </Form>
    </div>
  );
};
