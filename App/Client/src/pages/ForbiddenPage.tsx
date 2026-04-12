import './ForbiddenPage.scss';

import React from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';

import { PageShell } from '../components/PageShell';

export const ForbiddenPage: React.FC = () => {
  const { t } = useTranslation();

  return (
    <PageShell className="forbidden-page">
      <h1>{t('forbidden.title')}</h1>
      <p>{t('forbidden.message')}</p>
      <p>
        <Link to="/">{t('forbidden.goHome')}</Link>
      </p>
    </PageShell>
  );
};
