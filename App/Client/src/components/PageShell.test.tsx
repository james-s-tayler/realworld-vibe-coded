import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { PageShell } from './PageShell';

describe('PageShell', () => {
  it('renders children content', () => {
    render(<PageShell>Test content</PageShell>);
    expect(screen.getByText('Test content')).toBeInTheDocument();
  });

  it('applies custom className', () => {
    const { container } = render(
      <PageShell className="custom-page">Content</PageShell>
    );
    expect(container.firstChild).toHaveClass('page-shell', 'custom-page');
  });

  it('renders title when provided', () => {
    render(<PageShell title="Page Title">Content</PageShell>);
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Page Title');
  });

  it('renders subtitle when provided', () => {
    render(<PageShell subtitle="Page subtitle">Content</PageShell>);
    expect(screen.getByText('Page subtitle')).toBeInTheDocument();
  });

  it('renders both title and subtitle', () => {
    render(
      <PageShell title="Title" subtitle="Subtitle">
        Content
      </PageShell>
    );
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Title');
    expect(screen.getByText('Subtitle')).toBeInTheDocument();
  });

  it('renders banner when provided', () => {
    render(
      <PageShell banner={<div data-testid="banner">Banner content</div>}>
        Content
      </PageShell>
    );
    expect(screen.getByTestId('banner')).toBeInTheDocument();
    expect(screen.getByText('Banner content')).toBeInTheDocument();
  });

  describe('column layouts', () => {
    it('renders content for all layout variants', () => {
      const layouts = ['narrow', 'wide', 'full'] as const;
      for (const layout of layouts) {
        const { unmount } = render(
          <PageShell columnLayout={layout}>Content for {layout}</PageShell>
        );
        expect(screen.getByText(`Content for ${layout}`)).toBeInTheDocument();
        unmount();
      }
    });

    it('renders two-column layout with sidebar', () => {
      render(
        <PageShell columnLayout="two-column" sidebar={<div>Sidebar</div>}>
          Content
        </PageShell>
      );
      expect(screen.getByText('Content')).toBeInTheDocument();
      expect(screen.getByText('Sidebar')).toBeInTheDocument();
    });
  });

  describe('two-column layout with sidebar', () => {
    it('renders sidebar in two-column layout', () => {
      render(
        <PageShell columnLayout="two-column" sidebar={<div>Sidebar content</div>}>
          Main content
        </PageShell>
      );
      expect(screen.getByText('Sidebar content')).toBeInTheDocument();
      expect(screen.getByText('Main content')).toBeInTheDocument();
    });

    it('does not render sidebar when layout is not two-column', () => {
      render(
        <PageShell columnLayout="wide" sidebar={<div>Sidebar content</div>}>
          Main content
        </PageShell>
      );
      expect(screen.queryByText('Sidebar content')).not.toBeInTheDocument();
    });

    it('does not render sidebar when sidebar is not provided', () => {
      render(
        <PageShell columnLayout="two-column">Main content</PageShell>
      );
      expect(screen.getByText('Main content')).toBeInTheDocument();
    });
  });

  it('renders React nodes as title', () => {
    render(
      <PageShell title={<span data-testid="custom-title">Custom Title</span>}>
        Content
      </PageShell>
    );
    expect(screen.getByTestId('custom-title')).toBeInTheDocument();
  });

  it('renders React nodes as subtitle', () => {
    render(
      <PageShell subtitle={<span data-testid="custom-subtitle">Custom Subtitle</span>}>
        Content
      </PageShell>
    );
    expect(screen.getByTestId('custom-subtitle')).toBeInTheDocument();
  });
});
