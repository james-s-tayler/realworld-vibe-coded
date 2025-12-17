import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router';
import { Button, Tabs, TabList, Tab, TabPanels, TabPanel, Loading, InlineNotification, Pagination } from '@carbon/react';
import { Settings } from '@carbon/icons-react';
import { useAuth } from '../hooks/useAuth';
import { useRequireAuth } from '../hooks/useRequireAuth';
import { profilesApi } from '../api/profiles';
import { articlesApi } from '../api/articles';
import { ArticleList } from '../components/ArticleList';
import { PageShell } from '../components/PageShell';
import { ApiError } from '../api/client';
import type { Profile } from '../types/article';
import type { Article } from '../types/article';
import { DEFAULT_PROFILE_IMAGE } from '../constants';
import { truncateUsername } from '../utils/textUtils';
import './ProfilePage.css';

const DEFAULT_PAGE_SIZE = 20;
const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

interface ProfileBannerProps {
  profile: Profile;
  isOwnProfile: boolean;
  onFollow: () => void;
}

const ProfileBanner: React.FC<ProfileBannerProps> = ({ profile, isOwnProfile, onFollow }) => (
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
          {isOwnProfile ? (
            <Link to="/settings">
              <Button kind="ghost" size="sm" renderIcon={Settings}>
                Edit Profile Settings
              </Button>
            </Link>
          ) : (
            <Button
              kind="ghost"
              size="sm"
              onClick={onFollow}
            >
              {profile.following ? 'Unfollow' : 'Follow'} {truncateUsername(profile.username)}
            </Button>
          )}
        </div>
      </div>
    </div>
  </div>
);

export const ProfilePage: React.FC = () => {
  const { username } = useParams<{ username: string }>();
  const { user } = useAuth();
  const { requireAuth } = useRequireAuth();
  const [profile, setProfile] = useState<Profile | null>(null);
  const [articles, setArticles] = useState<Article[]>([]);
  const [articlesCount, setArticlesCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [loading, setLoading] = useState(true);
  const [articlesLoading, setArticlesLoading] = useState(false);
  const [activeTab, setActiveTab] = useState(0);
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
        setError('Failed to load profile');
      }
    } finally {
      setLoading(false);
    }
  }, [username]);

  const loadArticles = useCallback(async () => {
    if (!username) return;
    setArticlesLoading(true);
    try {
      const offset = (currentPage - 1) * pageSize;
      const params = activeTab === 0
        ? { author: username, limit: pageSize, offset }
        : { favorited: username, limit: pageSize, offset };
      const response = await articlesApi.listArticles(params);
      setArticles(response.articles);
      setArticlesCount(response.articlesCount);
    } catch (error) {
      console.error('Failed to load articles:', error);
    } finally {
      setArticlesLoading(false);
    }
  }, [username, activeTab, currentPage, pageSize]);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  useEffect(() => {
    loadArticles();
  }, [loadArticles]);

  const handleTabChange = (evt: { selectedIndex: number }) => {
    setActiveTab(evt.selectedIndex);
    setCurrentPage(1);
  };

  const handlePageChange = (evt: { page: number; pageSize: number }) => {
    if (evt.pageSize !== pageSize) {
      setPageSize(evt.pageSize);
      setCurrentPage(1);
    } else {
      setCurrentPage(evt.page);
    }
  };

  const handleFollow = async () => {
    if (!profile) return;
    try {
      await requireAuth(async () => {
        const response = profile.following
          ? await profilesApi.unfollowUser(profile.username)
          : await profilesApi.followUser(profile.username);
        setProfile(response.profile);
        return response;
      });
    } catch (error) {
      console.error('Failed to follow/unfollow:', error);
    }
  };

  const handleFavorite = async (slug: string) => {
    try {
      await requireAuth(async () => {
        const response = await articlesApi.favoriteArticle(slug);
        setArticles(articles.map(a => a.slug === slug ? response.article : a));
        return response;
      });
    } catch (error) {
      console.error('Failed to favorite article:', error);
    }
  };

  const handleUnfavorite = async (slug: string) => {
    try {
      await requireAuth(async () => {
        const response = await articlesApi.unfavoriteArticle(slug);
        setArticles(articles.map(a => a.slug === slug ? response.article : a));
        return response;
      });
    } catch (error) {
      console.error('Failed to unfavorite article:', error);
    }
  };

  if (loading) {
    return (
      <div className="profile-page loading">
        <Loading description="Loading profile..." withOverlay={false} />
      </div>
    );
  }

  if (error) {
    return (
      <PageShell className="profile-page">
        <InlineNotification
          kind="error"
          title="Error"
          subtitle={error}
          lowContrast
        />
      </PageShell>
    );
  }

  if (!profile) {
    return (
      <PageShell className="profile-page">
        <p>Profile not found</p>
      </PageShell>
    );
  }

  const isOwnProfile = user && user.username === profile.username;

  return (
    <PageShell
      className="profile-page"
      columnLayout="wide"
      banner={<ProfileBanner profile={profile} isOwnProfile={!!isOwnProfile} onFollow={handleFollow} />}
    >
      <Tabs selectedIndex={activeTab} onChange={handleTabChange}>
        <TabList aria-label="Profile tabs">
          <Tab>My Articles</Tab>
          <Tab>Favorited Articles</Tab>
        </TabList>
        <TabPanels>
          <TabPanel>
            <ArticleList
              articles={articles}
              loading={articlesLoading}
              onFavorite={handleFavorite}
              onUnfavorite={handleUnfavorite}
            />
            {articlesCount > 0 && (
              <Pagination
                page={currentPage}
                pageSize={pageSize}
                pageSizes={PAGE_SIZE_OPTIONS}
                totalItems={articlesCount}
                onChange={handlePageChange}
              />
            )}
          </TabPanel>
          <TabPanel>
            <ArticleList
              articles={articles}
              loading={articlesLoading}
              onFavorite={handleFavorite}
              onUnfavorite={handleUnfavorite}
            />
            {articlesCount > 0 && (
              <Pagination
                page={currentPage}
                pageSize={pageSize}
                pageSizes={PAGE_SIZE_OPTIONS}
                totalItems={articlesCount}
                onChange={handlePageChange}
              />
            )}
          </TabPanel>
        </TabPanels>
      </Tabs>
    </PageShell>
  );
};
