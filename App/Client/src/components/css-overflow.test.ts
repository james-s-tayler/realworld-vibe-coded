import { describe, it, expect } from 'vitest';
import { readFileSync } from 'fs';
import { join } from 'path';

describe('CSS Regression Tests - Component Text Overflow Prevention', () => {
  const readCSSFile = (relativePath: string): string => {
    const cssPath = join(__dirname, relativePath);
    return readFileSync(cssPath, 'utf-8');
  };

  describe('ArticlePreview.css', () => {
    it('should have overflow-wrap and word-break on .author-name', () => {
      const css = readCSSFile('./ArticlePreview.css');
      const authorBlock = css.match(/\.author-name\s*\{[^}]*\}/s)?.[0] || '';
      expect(authorBlock).toContain('overflow-wrap: break-word');
      expect(authorBlock).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .article-title', () => {
      const css = readCSSFile('./ArticlePreview.css');
      const titleBlock = css.match(/\.article-title\s*\{[^}]*\}/s)?.[0] || '';
      expect(titleBlock).toContain('overflow-wrap: break-word');
      expect(titleBlock).toContain('word-break: break-word');
    });

    it('should have overflow-wrap and word-break on .article-description', () => {
      const css = readCSSFile('./ArticlePreview.css');
      const descBlock = css.match(/\.article-description\s*\{[^}]*\}/s)?.[0] || '';
      expect(descBlock).toContain('overflow-wrap: break-word');
      expect(descBlock).toContain('word-break: break-word');
    });
  });

  describe('TagList.css', () => {
    it('should have overflow-wrap and word-break on .tag-pill', () => {
      const css = readCSSFile('./TagList.css');
      const tagBlock = css.match(/\.tag-pill\s*\{[^}]*\}/s)?.[0] || '';
      expect(tagBlock).toContain('overflow-wrap: break-word');
      expect(tagBlock).toContain('word-break: break-word');
    });
  });
});
