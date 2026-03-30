import React from 'react';
import { Link } from 'react-router';
import { Button } from '@carbon/react';
import { Settings, UserAvatar } from '@carbon/icons-react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { PageShell } from '../components/PageShell';

export const DashboardPage: React.FC = () => {
  const { t } = useTranslation();
  const { user } = useAuth();

  return (
    <PageShell className="dashboard-page">
      <h1>{user ? t('dashboard.welcome', { username: user.username }) : t('dashboard.welcomeGuest')}</h1>
      <p>{t('dashboard.subtitle')}</p>
      <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
        <Link to="/settings">
          <Button kind="primary" renderIcon={Settings}>
            {t('dashboard.settings')}
          </Button>
        </Link>
        {user && (
          <Link to={`/profile/${user.username}`}>
            <Button kind="secondary" renderIcon={UserAvatar}>
              {t('dashboard.viewProfile')}
            </Button>
          </Link>
        )}
      </div>
    </PageShell>
  );
};
