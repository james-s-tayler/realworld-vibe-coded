import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router';
import {
  Form,
  TextInput,
  Button,
  InlineNotification,
  Stack,
} from '@carbon/react';
import { useAuth } from '../hooks/useAuth';
import { ApiError } from '../api/client';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await login(email, password);
      navigate('/');
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

  return (
    <div style={{ maxWidth: '600px', margin: '2rem auto', padding: '0 1rem' }}>
      <h1 style={{ marginBottom: '2rem' }}>Sign In</h1>
      
      {error && (
        <InlineNotification
          kind="error"
          title="Login Failed"
          subtitle={error}
          onCloseButtonClick={() => setError(null)}
          style={{ marginBottom: '1rem' }}
        />
      )}

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
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
            labelText="Password"
            placeholder="Enter your password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            type="password"
          />

          <Button type="submit" disabled={loading}>
            {loading ? 'Signing in...' : 'Sign in'}
          </Button>

          <p style={{ marginTop: '1rem' }}>
            Don't have an account? <Link to="/register">Sign up</Link>
          </p>
        </Stack>
      </Form>
    </div>
  );
};
