import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { PageShell } from './PageShell';

describe('PageShell', () => {
  describe('rendering', () => {
    it('renders children content', () => {
      render(
        <PageShell>
          <p>Test content</p>
        </PageShell>
      );
      expect(screen.getByText('Test content')).toBeInTheDocument();
    });

    it('renders title when provided', () => {
      render(
        <PageShell title="Test Title">
          <p>Content</p>
        </PageShell>
      );
      expect(screen.getByRole('heading', { name: 'Test Title' })).toBeInTheDocument();
    });

    it('renders subtitle when provided', () => {
      render(
        <PageShell subtitle="Test subtitle">
          <p>Content</p>
        </PageShell>
      );
      expect(screen.getByText('Test subtitle')).toBeInTheDocument();
    });

    it('renders both title and subtitle when provided', () => {
      render(
        <PageShell title="Main Title" subtitle="Supporting text">
          <p>Content</p>
        </PageShell>
      );
      expect(screen.getByRole('heading', { name: 'Main Title' })).toBeInTheDocument();
      expect(screen.getByText('Supporting text')).toBeInTheDocument();
    });

    it('does not render title element when title is not provided', () => {
      render(
        <PageShell>
          <p>Content only</p>
        </PageShell>
      );
      expect(screen.queryByRole('heading')).not.toBeInTheDocument();
    });

    it('does not render subtitle element when subtitle is not provided', () => {
      const { container } = render(
        <PageShell>
          <p>Content only</p>
        </PageShell>
      );
      // Check there's no paragraph in text-xs-center that would be the subtitle
      const paragraphs = container.querySelectorAll('.text-xs-center');
      expect(paragraphs.length).toBe(0);
    });
  });

  describe('className prop', () => {
    it('applies custom className to the container', () => {
      render(
        <PageShell className="auth-page">
          <p>Content</p>
        </PageShell>
      );
      const shell = screen.getByTestId('page-shell');
      expect(shell).toHaveClass('auth-page');
    });

    it('works without className prop', () => {
      render(
        <PageShell>
          <p>Content</p>
        </PageShell>
      );
      const shell = screen.getByTestId('page-shell');
      expect(shell).toBeInTheDocument();
    });
  });

  describe('title ReactNode support', () => {
    it('renders title as ReactNode (JSX)', () => {
      render(
        <PageShell title={<span data-testid="custom-title">Custom Title</span>}>
          <p>Content</p>
        </PageShell>
      );
      expect(screen.getByTestId('custom-title')).toBeInTheDocument();
      expect(screen.getByText('Custom Title')).toBeInTheDocument();
    });
  });

  describe('subtitle ReactNode support', () => {
    it('renders subtitle as ReactNode (JSX)', () => {
      render(
        <PageShell subtitle={<a href="/test">Link subtitle</a>}>
          <p>Content</p>
        </PageShell>
      );
      expect(screen.getByRole('link', { name: 'Link subtitle' })).toBeInTheDocument();
    });
  });

  describe('accessibility', () => {
    it('has testid for testing', () => {
      render(
        <PageShell>
          <p>Content</p>
        </PageShell>
      );
      expect(screen.getByTestId('page-shell')).toBeInTheDocument();
    });
  });
});
