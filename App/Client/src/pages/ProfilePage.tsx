import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router';
import { Button, Loading, InlineNotification } from '@carbon/react';
import { Settings } from '@carbon/icons-react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { profilesApi } from '../api/profiles';
import { PageShell } from '../components/PageShell';
import { ApiError } from '../api/client';
import type { Profile } from '../types/profile';
import { DEFAULT_PROFILE_IMAGE } from '../constants';
import { truncateUsername } from '../utils/textUtils';
import './ProfilePage.css';

interface ProfileBannerProps {
  profile: Profile;
  isOwnProfile: boolean;
}

const ProfileBanner: React.FC<ProfileBannerProps> = ({ profile, isOwnProfile }) => {
  const { t } = useTranslation();

  return (
    <div className="user-info">
      <div className="container">
        <div className="row">
          <div className="col-xs-12 col-md-10 offset-md-1">
            <img
              src={profile.image || DEFAULT_PROFILE_IMAGE}
              alt={profile.username}
              className="user-img"
            />
            <h4 title={profile.username}>{truncateUsername(profile.username)}</h4>
            <p>{profile.bio}</p>
            {isOwnProfile && (
              <Link to="/settings">
                <Button kind="ghost" size="sm" renderIcon={Settings}>
                  {t('profile.editSettings')}
                </Button>
              </Link>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export const ProfilePage: React.FC = () => {
  const { t } = useTranslation();
  const { username } = useParams<{ username: string }>();
  const { user } = useAuth();
  const [profile, setProfile] = useState<Profile | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadProfile = useCallback(async () => {
    if (!username) return;
    setLoading(true);
    setError(null);
    try {
      const response = await profilesApi.getProfile(username);
      setProfile(response.profile);
    } catch (error) {
      console.error('Failed to load profile:', error);
      if (error instanceof ApiError) {
        setError(error.errors.join(', '));
      } else {
        setError(t('profile.failedToLoad'));
      }
    } finally {
      setLoading(false);
    }
  }, [username, t]);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  if (loading) {
    return (
      <div className="profile-page loading">
        <Loading description={t('profile.loading')} withOverlay={false} />
      </div>
    );
  }

  if (error) {
    return (
      <PageShell className="profile-page">
        <InlineNotification
          kind="error"
          title={t('profile.error')}
          subtitle={error}
          lowContrast
        />
      </PageShell>
    );
  }

  if (!profile) {
    return (
      <PageShell className="profile-page">
        <p>{t('profile.notFound')}</p>
      </PageShell>
    );
  }

  const isOwnProfile = user && user.username === profile.username;

  return (
    <PageShell
      className="profile-page"
      columnLayout="wide"
      banner={<ProfileBanner profile={profile} isOwnProfile={!!isOwnProfile} />}
    >
      {null}
    </PageShell>
  );
};
