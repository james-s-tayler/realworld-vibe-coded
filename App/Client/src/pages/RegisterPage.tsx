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
import { USER_CONSTRAINTS } from '../constants';
import { type AppError, normalizeError } from '../utils/errors';
import './AuthPages.css';

interface RegisterState {
  error: AppError | null;
}

export const RegisterPage: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { register } = useAuth();

  const [state, dispatch, isPending] = useActionState<RegisterState, FormData>(
    async (_prev, formData) => {
      try {
        await register(formData.get('email') as string, formData.get('password') as string);
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
      title={t('register.title')}
      subtitle={<Link to="/login">{t('register.haveAccount')}</Link>}
    >
      <ErrorDisplay
        error={state.error}
      />

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
          <TextInput
            id="email"
            name="email"
            labelText={t('register.email')}
            placeholder={t('register.email')}
            required
            type="email"
            maxLength={USER_CONSTRAINTS.EMAIL_MAX_LENGTH}
          />

          <PasswordInput
            id="password"
            name="password"
            labelText={t('register.password')}
            placeholder={t('register.password')}
            required
            minLength={USER_CONSTRAINTS.PASSWORD_MIN_LENGTH}
          />

          <Button type="submit" disabled={isPending} size="lg">
            {isPending ? t('register.submitting') : t('register.submit')}
          </Button>
        </Stack>
      </Form>
    </PageShell>
  );
};
