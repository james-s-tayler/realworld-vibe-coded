import { describe, it, expect } from 'vitest';
import { readFileSync } from 'fs';
import { join } from 'path';

describe('CSS Regression Tests - Text Overflow Prevention', () => {
  const readCSSFile = (relativePath: string): string => {
    const cssPath = join(__dirname, relativePath);
    return readFileSync(cssPath, 'utf-8');
  };

  describe('HomePage.css', () => {
    it('should have overflow-wrap and word-break on .banner-title', () => {
      const css = readCSSFile('./HomePage.css');
      const bannerTitleBlock = css.match(/\.banner-title\s*\{[^}]*\}/s)?.[0] || '';
      expect(bannerTitleBlock).toContain('overflow-wrap: break-word');
      expect(bannerTitleBlock).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .banner-subtitle', () => {
      const css = readCSSFile('./HomePage.css');
      const bannerSubtitleBlock = css.match(/\.banner-subtitle\s*\{[^}]*\}/s)?.[0] || '';
      expect(bannerSubtitleBlock).toContain('overflow-wrap: break-word');
      expect(bannerSubtitleBlock).toContain('word-break: break-word');
    });
  });

  describe('ArticlePage.css', () => {
    it('should have overflow-wrap and word-break on .article-page .banner h1', () => {
      const css = readCSSFile('./ArticlePage.css');
      const h1Block = css.match(/\.article-page\s+\.banner\s+h1\s*\{[^}]*\}/s)?.[0] || '';
      expect(h1Block).toContain('overflow-wrap: break-word');
      expect(h1Block).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .article-meta .author', () => {
      const css = readCSSFile('./ArticlePage.css');
      const authorBlock = css.match(/\.article-meta\s+\.author\s*\{[^}]*\}/s)?.[0] || '';
      expect(authorBlock).toContain('overflow-wrap: break-word');
      expect(authorBlock).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .article-body', () => {
      const css = readCSSFile('./ArticlePage.css');
      const bodyBlock = css.match(/\.article-body\s*\{[^}]*\}/s)?.[0] || '';
      expect(bodyBlock).toContain('overflow-wrap: break-word');
      expect(bodyBlock).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .tag-pill', () => {
      const css = readCSSFile('./ArticlePage.css');
      const tagBlock = css.match(/\.tag-pill\s*\{[^}]*\}/s)?.[0] || '';
      expect(tagBlock).toContain('overflow-wrap: break-word');
      expect(tagBlock).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .card-text', () => {
      const css = readCSSFile('./ArticlePage.css');
      const cardTextBlock = css.match(/\.card-text\s*\{[^}]*\}/s)?.[0] || '';
      expect(cardTextBlock).toContain('overflow-wrap: break-word');
      expect(cardTextBlock).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .comment-author-name', () => {
      const css = readCSSFile('./ArticlePage.css');
      const commentAuthorBlock = css.match(/\.comment-author-name\s*\{[^}]*\}/s)?.[0] || '';
      expect(commentAuthorBlock).toContain('overflow-wrap: break-word');
      expect(commentAuthorBlock).toContain('word-break: break-word');
    });
  });

  describe('ProfilePage.css', () => {
    it('should have overflow-wrap and word-break on .user-info h4', () => {
      const css = readCSSFile('./ProfilePage.css');
      const h4Block = css.match(/\.user-info\s+h4\s*\{[^}]*\}/s)?.[0] || '';
      expect(h4Block).toContain('overflow-wrap: break-word');
      expect(h4Block).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .user-info p', () => {
      const css = readCSSFile('./ProfilePage.css');
      const pBlock = css.match(/\.user-info\s+p\s*\{[^}]*\}/s)?.[0] || '';
      expect(pBlock).toContain('overflow-wrap: break-word');
      expect(pBlock).toContain('word-break: break-word');
    });
  });
});
