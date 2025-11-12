import React, { useState, useEffect, useCallback } from 'react';
import { Tabs, TabList, Tab, TabPanels, TabPanel, Tile } from '@carbon/react';
import { useAuth } from '../hooks/useAuth';
import { articlesApi } from '../api/articles';
import { tagsApi } from '../api/tags';
import { ArticleList } from '../components/ArticleList';
import { TagList } from '../components/TagList';
import type { Article } from '../types/article';
import './HomePage.css';

export const HomePage: React.FC = () => {
  const { user } = useAuth();
  const [articles, setArticles] = useState<Article[]>([]);
  const [tags, setTags] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [tagsLoading, setTagsLoading] = useState(false);
  const [activeTab, setActiveTab] = useState(0);
  const [selectedTag, setSelectedTag] = useState<string | null>(null);

  const loadTags = useCallback(async () => {
    setTagsLoading(true);
    try {
      const response = await tagsApi.getTags();
      setTags(response.tags);
    } catch (error) {
      console.error('Failed to load tags:', error);
    } finally {
      setTagsLoading(false);
    }
  }, []);

  const loadArticles = useCallback(async () => {
    setLoading(true);
    try {
      let response;
      if (activeTab === 0 && user) {
        // Your Feed
        response = await articlesApi.getFeed();
      } else if (selectedTag) {
        // Articles by Tag
        response = await articlesApi.listArticles({ tag: selectedTag });
      } else {
        // Global Feed
        response = await articlesApi.listArticles();
      }
      setArticles(response.articles);
    } catch (error) {
      console.error('Failed to load articles:', error);
    } finally {
      setLoading(false);
    }
  }, [activeTab, selectedTag, user]);

  useEffect(() => {
    loadTags();
  }, [loadTags]);

  useEffect(() => {
    loadArticles();
  }, [loadArticles]);

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

  const handleTagClick = (tag: string) => {
    setSelectedTag(tag);
    setActiveTab(user ? 2 : 1);
  };

  const handleTabChange = (evt: { selectedIndex: number }) => {
    setActiveTab(evt.selectedIndex);
    setSelectedTag(null);
  };

  return (
    <div className="home-page">
      <div className="banner">
        <div className="container">
          <h1 className="banner-title">conduit</h1>
          <p className="banner-subtitle">A place to share your <i>Angular</i> knowledge.</p>
        </div>
      </div>

      <div className="container page">
        <div className="row">
          <div className="col-md-9">
            <Tabs selectedIndex={activeTab} onChange={handleTabChange}>
              <TabList aria-label="Article feeds">
                {user && <Tab>Your Feed</Tab>}
                <Tab>Global Feed</Tab>
                {selectedTag && <Tab>#{selectedTag}</Tab>}
              </TabList>
              <TabPanels>
                {user && (
                  <TabPanel>
                    <ArticleList
                      articles={articles}
                      loading={loading}
                      onFavorite={handleFavorite}
                      onUnfavorite={handleUnfavorite}
                    />
                  </TabPanel>
                )}
                <TabPanel>
                  <ArticleList
                    articles={articles}
                    loading={loading}
                    onFavorite={handleFavorite}
                    onUnfavorite={handleUnfavorite}
                  />
                </TabPanel>
                {selectedTag && (
                  <TabPanel>
                    <ArticleList
                      articles={articles}
                      loading={loading}
                      onFavorite={handleFavorite}
                      onUnfavorite={handleUnfavorite}
                    />
                  </TabPanel>
                )}
              </TabPanels>
            </Tabs>
          </div>

          <div className="col-md-3">
            <Tile className="sidebar">
              <p className="sidebar-title">Popular Tags</p>
              <TagList tags={tags} loading={tagsLoading} onTagClick={handleTagClick} />
            </Tile>
          </div>
        </div>
      </div>
    </div>
  );
};
