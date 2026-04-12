import './AppHeader.scss';

import {
  Edit,
  Home,
  Login,
  Logout,
  Settings,
  UserAvatar,
  UserFollow,
  UserMultiple,
} from '@carbon/icons-react';
import {
  Header,
  HeaderContainer,
  HeaderMenuButton,
  HeaderName,
  SideNav,
  SideNavItems,
  SideNavLink,
  SkipToContent,
  Theme,
} from '@carbon/react';
import React, { useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useLocation, useNavigate } from 'react-router';

import { useAuth } from '../hooks/useAuth';
import { RequireRole } from './RequireRole';

export const AppHeader: React.FC = () => {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const closeRef = useRef<(() => void) | null>(null);

  const isActive = (path: string) => location.pathname === path;

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  // Close SideNav on route change (for mobile overlay)
  useEffect(() => {
    if (closeRef.current) {
      closeRef.current();
    }
  }, [location.pathname]);

  return (
    <HeaderContainer
      render={({ isSideNavExpanded, onClickSideNavExpand }: { isSideNavExpanded: boolean; onClickSideNavExpand: () => void }) => {
        closeRef.current = isSideNavExpanded ? onClickSideNavExpand : null;

        return (
          <Header aria-label={t('brand')}>
            <SkipToContent />
            <HeaderMenuButton
              aria-label={isSideNavExpanded ? t('nav.closeMenu') : t('nav.openMenu')}
              onClick={onClickSideNavExpand}
              isActive={isSideNavExpanded}
            />
            <HeaderName as={Link} to="/" prefix="">
              {t('brand')}
            </HeaderName>

            <Theme theme="g100">
              <SideNav
                aria-label={t('nav.sideNavLabel')}
                expanded={isSideNavExpanded}
                isChildOfHeader
                onSideNavBlur={onClickSideNavExpand}
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
                        {user.username}
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
          </Header>
        );
      }}
    />
  );
};
