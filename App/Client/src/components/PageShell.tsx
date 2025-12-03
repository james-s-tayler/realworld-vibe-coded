import React from 'react';
import './PageShell.css';

type ColumnLayout = 'narrow' | 'wide' | 'full' | 'two-column';

interface PageShellProps {
  /** Main page content */
  children: React.ReactNode;
  /** Page-specific class name (e.g. "auth-page", "editor-page", "home-page") */
  className?: string;
  /** Column layout: 'narrow' (col-md-6), 'wide' (col-md-10), 'full' (col-md-12), 'two-column' (9/3 split) */
  columnLayout?: ColumnLayout;
  /** Optional title displayed at top of content area */
  title?: React.ReactNode;
  /** Optional subtitle displayed below title */
  subtitle?: React.ReactNode;
  /** Optional banner slot - renders full-width above the main content container */
  banner?: React.ReactNode;
  /** Optional sidebar slot - only used when columnLayout is 'two-column' */
  sidebar?: React.ReactNode;
}

const columnClasses: Record<ColumnLayout, string> = {
  narrow: 'col-md-6 offset-md-3',
  wide: 'col-md-10 offset-md-1',
  full: 'col-md-12',
  'two-column': 'col-md-9',
};

export const PageShell: React.FC<PageShellProps> = ({
  children,
  className,
  columnLayout = 'full',
  title,
  subtitle,
  banner,
  sidebar,
}) => {
  const pageClassName = ['page-shell', className].filter(Boolean).join(' ');
  const mainColumnClass = columnClasses[columnLayout];

  return (
    <div className={pageClassName}>
      {banner && <div className="page-shell-banner">{banner}</div>}
      <div className="container page">
        <div className="row">
          <div className={`${mainColumnClass} col-xs-12`}>
            {title && <h1 className="text-xs-center">{title}</h1>}
            {subtitle && <p className="text-xs-center">{subtitle}</p>}
            {children}
          </div>
          {columnLayout === 'two-column' && sidebar && (
            <div className="col-md-3 col-xs-12">{sidebar}</div>
          )}
        </div>
      </div>
    </div>
  );
};
