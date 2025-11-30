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

  describe('columnLayout prop', () => {
    it('defaults to narrow column layout', () => {
      const { container } = render(
        <PageShell>
          <p>Content</p>
        </PageShell>
      );
      const columnDiv = container.querySelector('.col-md-6.offset-md-3');
      expect(columnDiv).toBeInTheDocument();
    });

    it('applies narrow column classes when columnLayout is narrow', () => {
      const { container } = render(
        <PageShell columnLayout="narrow">
          <p>Content</p>
        </PageShell>
      );
      const columnDiv = container.querySelector('.col-md-6.offset-md-3.col-xs-12');
      expect(columnDiv).toBeInTheDocument();
    });

    it('applies wide column classes when columnLayout is wide', () => {
      const { container } = render(
        <PageShell columnLayout="wide">
          <p>Content</p>
        </PageShell>
      );
      const columnDiv = container.querySelector('.col-md-10.offset-md-1.col-xs-12');
      expect(columnDiv).toBeInTheDocument();
    });

    it('applies full column classes when columnLayout is full', () => {
      const { container } = render(
        <PageShell columnLayout="full">
          <p>Content</p>
        </PageShell>
      );
      const columnDiv = container.querySelector('.col-md-12.col-xs-12');
      expect(columnDiv).toBeInTheDocument();
    });

    it('renders two-column layout with main content and sidebar', () => {
      const { container } = render(
        <PageShell
          columnLayout="two-column"
          sidebar={<div data-testid="sidebar">Sidebar Content</div>}
        >
          <p>Main Content</p>
        </PageShell>
      );
      const mainCol = container.querySelector('.col-md-9');
      const sidebarCol = container.querySelector('.col-md-3');
      expect(mainCol).toBeInTheDocument();
      expect(sidebarCol).toBeInTheDocument();
      expect(screen.getByText('Main Content')).toBeInTheDocument();
      expect(screen.getByTestId('sidebar')).toBeInTheDocument();
    });
  });

  describe('banner slot', () => {
    it('renders banner above the container when provided', () => {
      const { container } = render(
        <PageShell
          banner={<div data-testid="banner" className="my-banner">Banner Content</div>}
        >
          <p>Content</p>
        </PageShell>
      );
      expect(screen.getByTestId('banner')).toBeInTheDocument();
      expect(screen.getByText('Banner Content')).toBeInTheDocument();
      // Banner should be a sibling of container, not inside it
      const shell = screen.getByTestId('page-shell');
      const banner = container.querySelector('.my-banner');
      const containerPage = container.querySelector('.container.page');
      expect(shell.firstChild).toBe(banner);
      expect(shell.lastChild).toBe(containerPage);
    });

    it('does not render banner element when banner is not provided', () => {
      render(
        <PageShell>
          <p>Content</p>
        </PageShell>
      );
      expect(screen.queryByTestId('banner')).not.toBeInTheDocument();
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
