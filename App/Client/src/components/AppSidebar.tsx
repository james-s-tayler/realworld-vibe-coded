import React from 'react';
import { Link, useLocation, useNavigate } from 'react-router';
import {
  Theme,
  SideNav,
  SideNavItems,
  SideNavLink,
} from '@carbon/react';
import {
  Home,
  Edit,
  UserMultiple,
  Settings,
  UserAvatar,
  Login,
  UserFollow,
  Logout,
} from '@carbon/icons-react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { RequireRole } from './RequireRole';
import { truncateUsername } from '../utils/textUtils';
import './AppSidebar.css';

export const AppSidebar: React.FC = () => {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const isActive = (path: string) => location.pathname === path;

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <Theme theme="g100">
      <SideNav
        isFixedNav
        expanded
        isChildOfHeader={false}
        aria-label={t('nav.sideNavLabel')}
        className="app-sidebar"
      >
        <SideNavItems>
          {user ? (
            <>
              <SideNavLink
                as={Link}
                to="/"
                renderIcon={Home}
                isActive={isActive('/')}
              >
                {t('nav.home')}
              </SideNavLink>
              <SideNavLink
                as={Link}
                to="/editor"
                renderIcon={Edit}
                isActive={isActive('/editor')}
              >
                {t('nav.newArticle')}
              </SideNavLink>
              <RequireRole roles={['ADMIN']}>
                <SideNavLink
                  as={Link}
                  to="/users"
                  renderIcon={UserMultiple}
                  isActive={isActive('/users')}
                >
                  {t('nav.users')}
                </SideNavLink>
              </RequireRole>
              <SideNavLink
                as={Link}
                to="/settings"
                renderIcon={Settings}
                isActive={isActive('/settings')}
              >
                {t('nav.settings')}
              </SideNavLink>
              <div className="app-sidebar__profile-spacer" />
              <SideNavLink
                as={Link}
                to={`/profile/${user.username}`}
                renderIcon={UserAvatar}
                isActive={isActive(`/profile/${user.username}`)}
                title={user.username}
              >
                {truncateUsername(user.username)}
              </SideNavLink>
              <SideNavLink
                renderIcon={Logout}
                href="#"
                onClick={(e: React.MouseEvent) => {
                  e.preventDefault();
                  handleLogout();
                }}
              >
                {t('nav.logOut')}
              </SideNavLink>
            </>
          ) : (
            <>
              <SideNavLink
                as={Link}
                to="/login"
                renderIcon={Login}
                isActive={isActive('/login')}
              >
                {t('nav.signIn')}
              </SideNavLink>
              <SideNavLink
                as={Link}
                to="/register"
                renderIcon={UserFollow}
                isActive={isActive('/register')}
              >
                {t('nav.signUp')}
              </SideNavLink>
            </>
          )}
        </SideNavItems>
      </SideNav>
    </Theme>
  );
};
