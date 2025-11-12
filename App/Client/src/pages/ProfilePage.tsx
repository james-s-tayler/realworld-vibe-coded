import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router';
import { Button, Tabs, TabList, Tab, TabPanels, TabPanel, Loading } from '@carbon/react';
import { Settings } from '@carbon/icons-react';
import { useAuth } from '../hooks/useAuth';
import { profilesApi } from '../api/profiles';
import { articlesApi } from '../api/articles';
import { ArticleList } from '../components/ArticleList';
import type { Profile } from '../types/article';
import type { Article } from '../types/article';
import './ProfilePage.css';

export const ProfilePage: React.FC = () => {
  const { username } = useParams<{ username: string }>();
  const { user } = useAuth();
  const [profile, setProfile] = useState<Profile | null>(null);
  const [articles, setArticles] = useState<Article[]>([]);
  const [loading, setLoading] = useState(true);
  const [articlesLoading, setArticlesLoading] = useState(false);
  const [activeTab, setActiveTab] = useState(0);

  const loadProfile = useCallback(async () => {
    if (!username) return;
    setLoading(true);
    try {
      const response = await profilesApi.getProfile(username);
      setProfile(response.profile);
    } catch (error) {
      console.error('Failed to load profile:', error);
    } finally {
      setLoading(false);
    }
  }, [username]);

  const loadArticles = useCallback(async () => {
    if (!username) return;
    setArticlesLoading(true);
    try {
      const params = activeTab === 0
        ? { author: username }
        : { favorited: username };
      const response = await articlesApi.listArticles(params);
      setArticles(response.articles);
    } catch (error) {
      console.error('Failed to load articles:', error);
    } finally {
      setArticlesLoading(false);
    }
  }, [username, activeTab]);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  useEffect(() => {
    loadArticles();
  }, [loadArticles]);

  const handleFollow = async () => {
    if (!profile) return;
    try {
      const response = profile.following
        ? await profilesApi.unfollowUser(profile.username)
        : await profilesApi.followUser(profile.username);
      setProfile(response.profile);
    } catch (error) {
      console.error('Failed to follow/unfollow:', error);
    }
  };

  const handleFavorite = async (slug: string) => {
    try {
      const response = await articlesApi.favoriteArticle(slug);
      setArticles(articles.map(a => a.slug === slug ? response.article : a));
    } catch (error) {
      console.error('Failed to favorite article:', error);
    }
  };

  const handleUnfavorite = async (slug: string) => {
    try {
      const response = await articlesApi.unfavoriteArticle(slug);
      setArticles(articles.map(a => a.slug === slug ? response.article : a));
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

  if (!profile) {
    return (
      <div className="profile-page">
        <div className="container">
          <p>Profile not found</p>
        </div>
      </div>
    );
  }

  const isOwnProfile = user && user.username === profile.username;

  return (
    <div className="profile-page">
      <div className="user-info">
        <div className="container">
          <div className="row">
            <div className="col-xs-12 col-md-10 offset-md-1">
              <img
                src={profile.image || '/default-avatar.png'}
                alt={profile.username}
                className="user-img"
              />
              <h4>{profile.username}</h4>
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
                  onClick={handleFollow}
                >
                  {profile.following ? 'Unfollow' : 'Follow'} {profile.username}
                </Button>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="container">
        <div className="row">
          <div className="col-xs-12 col-md-10 offset-md-1">
            <Tabs selectedIndex={activeTab} onChange={(evt) => setActiveTab(evt.selectedIndex)}>
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
                </TabPanel>
                <TabPanel>
                  <ArticleList
                    articles={articles}
                    loading={articlesLoading}
                    onFavorite={handleFavorite}
                    onUnfavorite={handleUnfavorite}
                  />
                </TabPanel>
              </TabPanels>
            </Tabs>
          </div>
        </div>
      </div>
    </div>
  );
};
