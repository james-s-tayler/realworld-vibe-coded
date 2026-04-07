import React, { useState, useCallback } from 'react';
import { useNavigate, Link } from 'react-router';
import {
  Form,
  TextInput,
  Button,
  Stack,
} from '@carbon/react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { useApiCall } from '../hooks/useApiCall';
import { ErrorDisplay } from '../components/ErrorDisplay';
import { PageShell } from '../components/PageShell';
import './AuthPages.css';

export const LoginPage: React.FC = () => {
  const { t } = useTranslation();
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
      title={t('login.title')}
      subtitle={<Link to="/register">{t('login.needAccount')}</Link>}
    >
      <ErrorDisplay
        error={error}
        onClose={clearError}
      />

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
          <TextInput
            id="email"
            labelText={t('login.email')}
            placeholder={t('login.email')}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            type="email"
          />

          <TextInput
            id="password"
            labelText={t('login.password')}
            placeholder={t('login.password')}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            type="password"
          />

          <Button type="submit" disabled={loading} size="lg" className="pull-xs-right">
            {loading ? t('login.submitting') : t('login.submit')}
          </Button>
        </Stack>
      </Form>
    </PageShell>
  );
};
