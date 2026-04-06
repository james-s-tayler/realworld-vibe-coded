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
import { USER_CONSTRAINTS } from '../constants';
import './AuthPages.css';

export const RegisterPage: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { register } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const registerApi = useCallback(
    () => register(email, password),
    [register, email, password]
  );

  const { error, loading, execute, clearError } = useApiCall(registerApi, {
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
      title={t('register.title')}
      subtitle={<Link to="/login">{t('register.haveAccount')}</Link>}
    >
      <ErrorDisplay
        error={error}
        onClose={clearError}
      />

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
          <TextInput
            id="email"
            labelText={t('register.email')}
            placeholder={t('register.email')}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            type="email"
            maxLength={USER_CONSTRAINTS.EMAIL_MAX_LENGTH}
          />

          <TextInput
            id="password"
            labelText={t('register.password')}
            placeholder={t('register.password')}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            type="password"
            minLength={USER_CONSTRAINTS.PASSWORD_MIN_LENGTH}
          />

          <Button type="submit" disabled={loading} size="lg" className="pull-xs-right">
            {loading ? t('register.submitting') : t('register.submit')}
          </Button>
        </Stack>
      </Form>
    </PageShell>
  );
};
