import React, { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router';
import {
  Tabs,
  TabList,
  Tab,
  TabPanels,
  TabPanel,
  Tag,
  Pagination,
  Button,
} from '@carbon/react';
import { Favorite, FavoriteFilled } from '@carbon/icons-react';
import { useAuth } from '../hooks/useAuth';
import { articlesApi } from '../api/articles';
import { tagsApi } from '../api/tags';
import { PageShell } from '../components/PageShell';
import { DEFAULT_PROFILE_IMAGE } from '../constants';
import type { ArticleDto } from '../api/generated/models/index.js';
import './DashboardPage.css';

const PAGE_SIZE = 20;

export const DashboardPage: React.FC = () => {
  const { user } = useAuth();
  const [articles, setArticles] = useState<ArticleDto[]>([]);
  const [articlesCount, setArticlesCount] = useState(0);
  const [tags, setTags] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedTab, setSelectedTab] = useState(user ? 0 : 0);
  const [selectedTag, setSelectedTag] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [favoritingSlug, setFavoritingSlug] = useState<string | null>(null);

  const feedTab = user ? 'feed' : null;
  const tabOrder = [
    ...(feedTab ? ['feed'] : []),
    'global',
    ...(selectedTag ? ['tag'] : []),
  ];

  const currentTab = tabOrder[selectedTab] || 'global';

  const loadArticles = useCallback(async () => {
    setLoading(true);
    try {
      const offset = (page - 1) * PAGE_SIZE;
      let result;

      if (currentTab === 'feed') {
        result = await articlesApi.getFeed({ limit: PAGE_SIZE, offset });
      } else if (currentTab === 'tag' && selectedTag) {
        result = await articlesApi.listArticles({ tag: selectedTag, limit: PAGE_SIZE, offset });
      } else {
        result = await articlesApi.listArticles({ limit: PAGE_SIZE, offset });
      }

      setArticles(result.articles ?? []);
      setArticlesCount(result.articlesCount ?? 0);
    } catch {
      setArticles([]);
      setArticlesCount(0);
    } finally {
      setLoading(false);
    }
  }, [currentTab, selectedTag, page]);

  const loadTags = useCallback(async () => {
    try {
      const result = await tagsApi.getTags();
      setTags(result.tags ?? []);
    } catch {
      setTags([]);
    }
  }, []);

  useEffect(() => {
    loadArticles();
  }, [loadArticles]);

  useEffect(() => {
    loadTags();
  }, [loadTags]);

  const handleTabChange = (evt: { selectedIndex: number }) => {
    setSelectedTab(evt.selectedIndex);
    setPage(1);
    if (tabOrder[evt.selectedIndex] !== 'tag') {
      setSelectedTag(null);
    }
  };

  const handleTagClick = (tag: string) => {
    setSelectedTag(tag);
    const tagIndex = tabOrder.indexOf('tag');
    if (tagIndex >= 0) {
      setSelectedTab(tagIndex);
    } else {
      setSelectedTab(tabOrder.length);
    }
    setPage(1);
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
      // ignore favorite errors
    } finally {
      setFavoritingSlug(null);
    }
  };

  const handlePageChange = (evt: { page: number; pageSize: number }) => {
    setPage(evt.page);
  };

  const formatDate = (date: Date | null | undefined) => {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const sidebar = (
    <div className="sidebar">
      <p>Popular Tags</p>
      <div className="tag-list">
        {tags.map((tag) => (
          <Tag
            key={tag}
            type="cool-gray"
            size="sm"
            onClick={() => handleTagClick(tag)}
            style={{ cursor: 'pointer' }}
          >
            {tag}
          </Tag>
        ))}
        {tags.length === 0 && <p>No tags found</p>}
      </div>
    </div>
  );

  const renderArticleList = () => {
    if (loading) {
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
        <Pagination
          totalItems={articlesCount}
          pageSize={PAGE_SIZE}
          pageSizes={[PAGE_SIZE]}
          page={page}
          onChange={handlePageChange}
        />
      </>
    );
  };

  return (
    <PageShell className="home-page" columnLayout="two-column" sidebar={sidebar}>
      <Tabs selectedIndex={selectedTab} onChange={handleTabChange}>
        <TabList aria-label="Article feeds">
          {user && <Tab>Your Feed</Tab>}
          <Tab>Global Feed</Tab>
          {selectedTag && <Tab>#{selectedTag}</Tab>}
        </TabList>
        <TabPanels>
          {user && <TabPanel>{renderArticleList()}</TabPanel>}
          <TabPanel>{renderArticleList()}</TabPanel>
          {selectedTag && <TabPanel>{renderArticleList()}</TabPanel>}
        </TabPanels>
      </Tabs>
    </PageShell>
  );
};
