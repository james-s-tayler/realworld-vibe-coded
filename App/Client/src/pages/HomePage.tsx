import React from 'react';
import { Link } from 'react-router';
import { Button, Stack, Tile } from '@carbon/react';
import { useAuth } from '../hooks/useAuth';

export const HomePage: React.FC = () => {
  const { user } = useAuth();

  return (
    <div style={{ maxWidth: '800px', margin: '2rem auto', padding: '0 1rem' }}>
      <h1 style={{ marginBottom: '2rem' }}>Conduit</h1>
      
      <Tile style={{ padding: '2rem' }}>
        {user ? (
          <Stack gap={6}>
            <h2>Welcome back, {user.username}!</h2>
            <p>You are logged in to Conduit.</p>
            <div>
              <Link to="/profile">
                <Button>View Profile</Button>
              </Link>
            </div>
          </Stack>
        ) : (
          <Stack gap={6}>
            <h2>Welcome to Conduit</h2>
            <p>A place to share your knowledge.</p>
            <div style={{ display: 'flex', gap: '1rem' }}>
              <Link to="/login">
                <Button>Sign In</Button>
              </Link>
              <Link to="/register">
                <Button kind="secondary">Sign Up</Button>
              </Link>
            </div>
          </Stack>
        )}
      </Tile>
    </div>
  );
};
