import React from 'react';
import { Link } from 'react-router';
import {
  Header,
  HeaderContainer,
  HeaderName,
} from '@carbon/react';
import './AppHeader.css';

export const AppHeader: React.FC = () => {
  return (
    <HeaderContainer
      render={() => (
        <Header aria-label="Conduit">
          <HeaderName as={Link} to="/" prefix="">
            conduit
          </HeaderName>
        </Header>
      )}
    />
  );
};
