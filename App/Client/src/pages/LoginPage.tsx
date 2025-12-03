import React, { useState, useCallback } from 'react';
import { useNavigate, Link } from 'react-router';
import {
  Form,
  TextInput,
  Button,
  Stack,
} from '@carbon/react';
import { useAuth } from '../hooks/useAuth';
import { useApiCall } from '../hooks/useApiCall';
import { ErrorDisplay } from '../components/ErrorDisplay';
import { PageShell } from '../components/PageShell';
import './AuthPages.css';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const loginApi = useCallback(
    () => login(email, password),
    [login, email, password]
  );

  const { error, loading, execute, clearError } = useApiCall(loginApi, {
    onSuccess: () => navigate('/'),
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await execute();
  };

  return (
    <PageShell
      className="auth-page"
      columnLayout="narrow"
      title="Sign in"
      subtitle={<Link to="/register">Need an account?</Link>}
    >
      <ErrorDisplay
        error={error}
        onClose={clearError}
      />

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
          <TextInput
            id="email"
            labelText="Email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            type="email"
          />

          <TextInput
            id="password"
            labelText="Password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            type="password"
          />

          <Button type="submit" disabled={loading} size="lg" className="pull-xs-right">
            {loading ? 'Signing in...' : 'Sign in'}
          </Button>
        </Stack>
      </Form>
    </PageShell>
  );
};
