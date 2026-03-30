import React from 'react';
import { Link } from 'react-router';
import {
  Header,
  HeaderContainer,
  HeaderName,
} from '@carbon/react';
import { useTranslation } from 'react-i18next';
import './AppHeader.css';

export const AppHeader: React.FC = () => {
  const { t } = useTranslation();
  return (
    <HeaderContainer
      render={() => (
        <Header aria-label="Conduit">
          <HeaderName as={Link} to="/" prefix="">
            {t('brand')}
          </HeaderName>
        </Header>
      )}
    />
  );
};
