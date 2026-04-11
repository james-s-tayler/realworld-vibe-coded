import React from 'react';
import { Link } from 'react-router';
import { Button, ToastNotification } from '@carbon/react';
import { Settings, UserAvatar } from '@carbon/icons-react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { useFeatureFlag } from '../hooks/useFeatureFlag';
import { FEATURE_FLAGS } from '../featureFlags';
import { PageShell } from '../components/PageShell';
import './DashboardPage.css';

export const DashboardPage: React.FC = () => {
  const { t } = useTranslation();
  const { user } = useAuth();
  const showBanner = useFeatureFlag(FEATURE_FLAGS.DASHBOARD_BANNER);

  return (
    <PageShell className="dashboard-page">
      {showBanner && (
        <ToastNotification
          kind="info"
          title={t('dashboard.bannerTitle')}
          subtitle={t('dashboard.bannerMessage')}
          lowContrast
          hideCloseButton
        />
      )}
      <h1>{user ? t('dashboard.welcome', { username: user.username }) : t('dashboard.welcomeGuest')}</h1>
      <p>{t('dashboard.subtitle')}</p>
      <div className="dashboard-actions">
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
