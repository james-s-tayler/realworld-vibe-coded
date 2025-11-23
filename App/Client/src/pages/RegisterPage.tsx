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
import './AuthPages.css';

export const RegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const { register } = useAuth();
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await register(email, username, password);
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
    <div className="auth-page">
      <div className="container page">
        <div className="row">
          <div className="col-md-6 offset-md-3 col-xs-12">
            <h1 className="text-xs-center">Sign up</h1>
            <p className="text-xs-center">
              <Link to="/login">Have an account?</Link>
            </p>

            {error && (
              <InlineNotification
                kind="error"
                title="Registration Failed"
                subtitle={error}
                onCloseButtonClick={() => setError(null)}
                style={{ marginBottom: '1rem' }}
              />
            )}

            <Form onSubmit={handleSubmit}>
              <Stack gap={6}>
                <TextInput
                  id="username"
                  labelText="Username"
                  placeholder="Username"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  required
                />

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
                  {loading ? 'Creating account...' : 'Sign up'}
                </Button>
              </Stack>
            </Form>
          </div>
        </div>
      </div>
    </div>
  );
};
