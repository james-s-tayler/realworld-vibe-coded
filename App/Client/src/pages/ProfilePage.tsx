import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router';
import {
  Button,
  Loading,
  InlineNotification,
  Tabs,
  TabList,
  Tab,
  TabPanels,
  TabPanel,
  Tag,
  Pagination,
} from '@carbon/react';
import { Settings, Add, Subtract, Favorite, FavoriteFilled } from '@carbon/icons-react';
import { useAuth } from '../hooks/useAuth';
import { profilesApi } from '../api/profiles';
import { articlesApi } from '../api/articles';
import { PageShell } from '../components/PageShell';
import { ApiError } from '../api/client';
import type { Profile } from '../types/profile';
import type { ArticleDto } from '../api/generated/models/index.js';
import { DEFAULT_PROFILE_IMAGE } from '../constants';
import { truncateUsername } from '../utils/textUtils';
import './ProfilePage.css';

const PAGE_SIZE = 20;

interface ProfileBannerProps {
  profile: Profile & { following?: boolean };
  isOwnProfile: boolean;
  onFollowToggle: () => void;
  togglingFollow: boolean;
}

const ProfileBanner: React.FC<ProfileBannerProps> = ({
  profile,
  isOwnProfile,
  onFollowToggle,
  togglingFollow,
}) => (
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
              kind={profile.following ? 'secondary' : 'ghost'}
              size="sm"
              renderIcon={profile.following ? Subtract : Add}
              onClick={onFollowToggle}
              disabled={togglingFollow}
            >
              {profile.following
                ? `Unfollow ${profile.username}`
                : `Follow ${profile.username}`}
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
  const [profile, setProfile] = useState<(Profile & { following?: boolean }) | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [togglingFollow, setTogglingFollow] = useState(false);
  const [selectedTab, setSelectedTab] = useState(0);
  const [articles, setArticles] = useState<ArticleDto[]>([]);
  const [articlesCount, setArticlesCount] = useState(0);
  const [articlesLoading, setArticlesLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [favoritingSlug, setFavoritingSlug] = useState<string | null>(null);

  const loadProfile = useCallback(async () => {
    if (!username) return;
    setLoading(true);
    setError(null);
    try {
      const response = await profilesApi.getProfile(username);
      setProfile(response.profile as Profile & { following?: boolean });
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
      const offset = (page - 1) * PAGE_SIZE;
      const params =
        selectedTab === 0
          ? { author: username, limit: PAGE_SIZE, offset }
          : { favorited: username, limit: PAGE_SIZE, offset };

      const result = await articlesApi.listArticles(params);
      setArticles(result.articles ?? []);
      setArticlesCount(result.articlesCount ?? 0);
    } catch {
      setArticles([]);
      setArticlesCount(0);
    } finally {
      setArticlesLoading(false);
    }
  }, [username, selectedTab, page]);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  useEffect(() => {
    loadArticles();
  }, [loadArticles]);

  const handleFollowToggle = async () => {
    if (!profile || togglingFollow) return;
    setTogglingFollow(true);
    try {
      const result = profile.following
        ? await profilesApi.unfollowUser(profile.username)
        : await profilesApi.followUser(profile.username);
      setProfile(result.profile as Profile & { following?: boolean });
    } catch {
      // ignore
    } finally {
      setTogglingFollow(false);
    }
  };

  const handleTabChange = (evt: { selectedIndex: number }) => {
    setSelectedTab(evt.selectedIndex);
    setPage(1);
  };

  const handlePageChange = (evt: { page: number; pageSize: number }) => {
    setPage(evt.page);
  };

  const handleFavorite = async (article: ArticleDto) => {
    if (!article.slug || favoritingSlug) return;
    setFavoritingSlug(article.slug);
    try {
      const result = article.favorited
        ? await articlesApi.unfavoriteArticle(article.slug)
        : await articlesApi.favoriteArticle(article.slug);
      if (result.article) {
        setArticles((prev) =>
          prev.map((a) => (a.slug === article.slug ? result.article! : a))
        );
      }
    } catch {
      // ignore
    } finally {
      setFavoritingSlug(null);
    }
  };

  const formatDate = (date: Date | null | undefined) => {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
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

  const renderArticleList = () => {
    if (articlesLoading) {
      return (
        <div className="article-preview">
          <p>Loading articles...</p>
        </div>
      );
    }

    if (articles.length === 0) {
      return (
        <div className="article-preview">
          <p>No articles are here... yet.</p>
        </div>
      );
    }

    return (
      <>
        {articles.map((article) => (
          <div key={article.slug} className="article-preview">
            <div className="article-meta">
              <Link to={`/profile/${article.author?.username}`} className="author-info">
                <img
                  src={article.author?.image || DEFAULT_PROFILE_IMAGE}
                  alt={article.author?.username ?? ''}
                />
                <div className="info">
                  <span className="author">{article.author?.username}</span>
                  <span className="date">{formatDate(article.createdAt)}</span>
                </div>
              </Link>
              <Button
                className="favorite-button"
                kind={article.favorited ? 'primary' : 'ghost'}
                size="sm"
                renderIcon={article.favorited ? FavoriteFilled : Favorite}
                hasIconOnly
                iconDescription={article.favorited ? 'Unfavorite' : 'Favorite'}
                onClick={() => handleFavorite(article)}
                disabled={favoritingSlug === article.slug}
              >
                {article.favoritesCount ?? 0}
              </Button>
            </div>
            <Link to={`/article/${article.slug}`} className="article-link">
              <h2>{article.title}</h2>
              <p>{article.description}</p>
              <span>Read more...</span>
            </Link>
            {article.tagList && article.tagList.length > 0 && (
              <ul className="tag-list">
                {article.tagList.map((tag) => (
                  <li key={tag}>
                    <Tag type="cool-gray" size="sm">
                      {tag}
                    </Tag>
                  </li>
                ))}
              </ul>
            )}
          </div>
        ))}
        {articlesCount > PAGE_SIZE && (
          <Pagination
            totalItems={articlesCount}
            pageSize={PAGE_SIZE}
            pageSizes={[PAGE_SIZE]}
            page={page}
            onChange={handlePageChange}
          />
        )}
      </>
    );
  };

  return (
    <PageShell
      className="profile-page"
      columnLayout="wide"
      banner={
        <ProfileBanner
          profile={profile}
          isOwnProfile={!!isOwnProfile}
          onFollowToggle={handleFollowToggle}
          togglingFollow={togglingFollow}
        />
      }
    >
      <Tabs selectedIndex={selectedTab} onChange={handleTabChange}>
        <TabList aria-label="Profile article tabs">
          <Tab>My Articles</Tab>
          <Tab>Favorited Articles</Tab>
        </TabList>
        <TabPanels>
          <TabPanel>{renderArticleList()}</TabPanel>
          <TabPanel>{renderArticleList()}</TabPanel>
        </TabPanels>
      </Tabs>
    </PageShell>
  );
};
