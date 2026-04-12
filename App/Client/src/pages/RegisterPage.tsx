import {
  Button,
  Form,
  InlineLoading,
  PasswordInput,
  Stack,
  TextInput,
} from '@carbon/react';
import React, { startTransition,useActionState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link,useNavigate } from 'react-router';

import { ErrorDisplay } from '../components/ErrorDisplay';
import { PageShell } from '../components/PageShell';
import { USER_CONSTRAINTS } from '../constants';
import { useAuth } from '../hooks/useAuth';
import { type AppError, normalizeError } from '../utils/errors';

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
      columnLayout="wide"
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

          {isPending ? (
            <InlineLoading status="active" description={t('register.submitting')} />
          ) : (
            <Button type="submit" size="lg">
              {t('register.submit')}
            </Button>
          )}
        </Stack>
      </Form>
    </PageShell>
  );
};
