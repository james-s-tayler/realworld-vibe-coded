import React from 'react';
import { Link, useLocation } from 'react-router';
import {
  Header,
  HeaderContainer,
  HeaderName,
  HeaderNavigation,
  HeaderMenuItem,
} from '@carbon/react';
import { Edit } from '@carbon/icons-react';
import { useAuth } from '../hooks/useAuth';
import { truncateUsername } from '../utils/textUtils';
import './AppHeader.css';

export const AppHeader: React.FC = () => {
  const { user } = useAuth();
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <HeaderContainer
      render={() => (
        <Header aria-label="Conduit">
          <HeaderName as={Link} to="/" prefix="">
            conduit
          </HeaderName>
          <HeaderNavigation aria-label="Main navigation">
            {user && (
              <HeaderMenuItem
                as={Link}
                to="/"
                isActive={isActive('/')}
              >
                Home
              </HeaderMenuItem>
            )}
            {user ? (
              <>
                <HeaderMenuItem
                  as={Link}
                  to="/editor"
                  isActive={isActive('/editor')}
                >
                  <Edit size={16} style={{ marginRight: '4px' }} />
                  New Article
                </HeaderMenuItem>
                <HeaderMenuItem
                  as={Link}
                  to="/settings"
                  isActive={isActive('/settings')}
                >
                  Settings
                </HeaderMenuItem>
                <HeaderMenuItem
                  as={Link}
                  to={`/profile/${user.username}`}
                  isActive={isActive(`/profile/${user.username}`)}
                  title={user.username}
                >
                  {truncateUsername(user.username)}
                </HeaderMenuItem>
              </>
            ) : (
              <>
                <HeaderMenuItem
                  as={Link}
                  to="/login"
                  isActive={isActive('/login')}
                >
                  Sign in
                </HeaderMenuItem>
                <HeaderMenuItem
                  as={Link}
                  to="/register"
                  isActive={isActive('/register')}
                >
                  Sign up
                </HeaderMenuItem>
              </>
            )}
          </HeaderNavigation>
        </Header>
      )}
    />
  );
};
