import React, { useState, useEffect, useCallback } from 'react';
import {
  Form,
  TextInput,
  TextArea,
  PasswordInput,
  Button,
  Stack,
  Dropdown,
} from '@carbon/react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { useToast } from '../hooks/useToast';
import { useApiCall } from '../hooks/useApiCall';
import { ErrorDisplay } from '../components/ErrorDisplay';
import { PageShell } from '../components/PageShell';
import { USER_CONSTRAINTS, SUPPORTED_LANGUAGES } from '../constants';
import './SettingsPage.css';

export const SettingsPage: React.FC = () => {
  const { t, i18n } = useTranslation();
  const { user, updateUser } = useAuth();
  const { showToast } = useToast();
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [bio, setBio] = useState('');
  const [image, setImage] = useState('');
  const [password, setPassword] = useState('');
  const [language, setLanguage] = useState(i18n.language);

  useEffect(() => {
    if (user) {
      setEmail(user.email);
      setUsername(user.username);
      setBio(user.bio || '');
      setImage(user.image || '');
    }
  }, [user]);

  const updateApi = useCallback(() => {
    const updates: {
      email?: string;
      username?: string;
      bio?: string;
      image?: string;
      password?: string;
      language?: string;
    } = {};

    if (email !== user?.email) updates.email = email;
    if (username !== user?.username) updates.username = username;
    if (bio !== (user?.bio || '')) updates.bio = bio;
    if (image !== (user?.image || '')) updates.image = image;
    if (password) updates.password = password;
    updates.language = language;

    return updateUser(updates);
  }, [email, username, bio, image, password, language, user, updateUser]);

  const { error, loading, execute, clearError } = useApiCall(updateApi, {
    onSuccess: () => {
      showToast({ kind: 'success', title: t('settings.success'), subtitle: t('settings.successMessage') });
      setPassword('');
      i18n.changeLanguage(language);
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await execute();
  };

  if (!user) {
    return null;
  }

  return (
    <PageShell
      className="settings-page"
      columnLayout="narrow"
      title={t('settings.title')}
    >
      <ErrorDisplay
        error={error}
        onClose={clearError}
      />

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
          <TextInput
            id="image"
            labelText={t('settings.imageUrlLabel')}
            hideLabel
            placeholder={t('settings.imageUrl')}
            value={image}
            onChange={(e) => setImage(e.target.value)}
            maxLength={USER_CONSTRAINTS.IMAGE_URL_MAX_LENGTH}
          />

          <TextInput
            id="username"
            labelText={t('settings.usernameLabel')}
            hideLabel
            placeholder={t('settings.username')}
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            minLength={USER_CONSTRAINTS.USERNAME_MIN_LENGTH}
            maxLength={USER_CONSTRAINTS.USERNAME_MAX_LENGTH}
          />

          <TextArea
            id="bio"
            labelText={t('settings.bioLabel')}
            hideLabel
            placeholder={t('settings.bio')}
            value={bio}
            onChange={(e) => setBio(e.target.value)}
            rows={8}
            maxLength={USER_CONSTRAINTS.BIO_MAX_LENGTH}
          />

          <TextInput
            id="email"
            labelText={t('settings.emailLabel')}
            hideLabel
            placeholder={t('settings.email')}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            type="email"
            maxLength={USER_CONSTRAINTS.EMAIL_MAX_LENGTH}
          />

          <PasswordInput
            id="password"
            labelText={t('settings.newPasswordLabel')}
            hideLabel
            placeholder={t('settings.newPassword')}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            minLength={USER_CONSTRAINTS.PASSWORD_MIN_LENGTH}
          />

          <Dropdown
            id="language"
            titleText={t('settings.language')}
            label={t('settings.language')}
            items={[...SUPPORTED_LANGUAGES]}
            itemToString={(item: typeof SUPPORTED_LANGUAGES[number]) => item?.label ?? ''}
            selectedItem={SUPPORTED_LANGUAGES.find((l) => l.id === language) ?? SUPPORTED_LANGUAGES[0]}
            onChange={({ selectedItem }: { selectedItem: typeof SUPPORTED_LANGUAGES[number] }) => {
              if (selectedItem) setLanguage(selectedItem.id);
            }}
          />

          <Button type="submit" disabled={loading} size="lg" className="pull-xs-right">
            {loading ? t('settings.submitting') : t('settings.submit')}
          </Button>
        </Stack>
      </Form>

    </PageShell>
  );
};
