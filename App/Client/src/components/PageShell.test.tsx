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
    it('uses full layout by default', () => {
      const { container } = render(<PageShell>Content</PageShell>);
      expect(container.querySelector('.col-md-12')).toBeInTheDocument();
    });

    it('applies narrow layout (col-md-6)', () => {
      const { container } = render(
        <PageShell columnLayout="narrow">Content</PageShell>
      );
      expect(container.querySelector('.col-md-6.offset-md-3')).toBeInTheDocument();
    });

    it('applies wide layout (col-md-10)', () => {
      const { container } = render(
        <PageShell columnLayout="wide">Content</PageShell>
      );
      expect(container.querySelector('.col-md-10.offset-md-1')).toBeInTheDocument();
    });

    it('applies full layout (col-md-12)', () => {
      const { container } = render(
        <PageShell columnLayout="full">Content</PageShell>
      );
      expect(container.querySelector('.col-md-12')).toBeInTheDocument();
    });

    it('applies two-column layout (col-md-9)', () => {
      const { container } = render(
        <PageShell columnLayout="two-column" sidebar={<div>Sidebar</div>}>
          Content
        </PageShell>
      );
      expect(container.querySelector('.col-md-9')).toBeInTheDocument();
      expect(container.querySelector('.col-md-3')).toBeInTheDocument();
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

    it('does not render sidebar column when sidebar is not provided', () => {
      const { container } = render(
        <PageShell columnLayout="two-column">Main content</PageShell>
      );
      expect(container.querySelector('.col-md-3')).not.toBeInTheDocument();
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
