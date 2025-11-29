import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router';
import {
  Form,
  TextInput,
  Button,
  Stack,
} from '@carbon/react';
import { useAuth } from '../hooks/useAuth';
import { ApiError } from '../api/client';
import { ErrorDisplay } from '../components/ErrorDisplay';
import './AuthPages.css';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<ApiError | Error | null>(null);
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
        setError(err);
      } else if (err instanceof Error) {
        setError(err);
      } else {
        setError(new Error('An unexpected error occurred'));
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="container page">
        <div className="row">
          <div className="col-md-6 offset-md-3 col-xs-12">
            <h1 className="text-xs-center">Sign in</h1>
            <p className="text-xs-center">
              <Link to="/register">Need an account?</Link>
            </p>

            <ErrorDisplay
              error={error}
              title="Login Failed"
              onClose={() => setError(null)}
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
          </div>
        </div>
      </div>
    </div>
  );
};
