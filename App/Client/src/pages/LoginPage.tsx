import React, { useActionState, startTransition } from 'react';
import { useNavigate, Link } from 'react-router';
import {
  Form,
  TextInput,
  PasswordInput,
  Button,
  Stack,
} from '@carbon/react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { ErrorDisplay } from '../components/ErrorDisplay';
import { PageShell } from '../components/PageShell';
import { type AppError, normalizeError } from '../utils/errors';
import './AuthPages.scss';

interface LoginState {
  error: AppError | null;
}

export const LoginPage: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { login } = useAuth();

  const [state, dispatch, isPending] = useActionState<LoginState, FormData>(
    async (_prev, formData) => {
      try {
        await login(formData.get('email') as string, formData.get('password') as string);
        navigate('/');
        return { error: null };
      } catch (err) {
        return { error: normalizeError(err) };
      }
    },
    { error: null },
  );

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    startTransition(() => {
      dispatch(formData);
    });
  };

  return (
    <PageShell
      className="auth-page"
      columnLayout="narrow"
      title={t('login.title')}
      subtitle={<Link to="/register">{t('login.needAccount')}</Link>}
    >
      <ErrorDisplay
        error={state.error}
      />

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
          <TextInput
            id="email"
            name="email"
            labelText={t('login.email')}
            placeholder={t('login.email')}
            required
            type="email"
          />

          <PasswordInput
            id="password"
            name="password"
            labelText={t('login.password')}
            placeholder={t('login.password')}
            required
          />

          <Button type="submit" disabled={isPending} size="lg">
            {isPending ? t('login.submitting') : t('login.submit')}
          </Button>
        </Stack>
      </Form>
    </PageShell>
  );
};
