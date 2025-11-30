import React from 'react';

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

const columnClasses: Record<Exclude<ColumnLayout, 'two-column'>, string> = {
  narrow: 'col-md-6 offset-md-3 col-xs-12',
  wide: 'col-md-10 offset-md-1 col-xs-12',
  full: 'col-md-12 col-xs-12',
};

export const PageShell: React.FC<PageShellProps> = ({
  children,
  className,
  columnLayout = 'narrow',
  title,
  subtitle,
  banner,
  sidebar,
}) => {
  const renderContent = () => {
    if (columnLayout === 'two-column') {
      return (
        <div className="row">
          <div className="col-md-9">
            {title && <h1 className="text-xs-center">{title}</h1>}
            {subtitle && <p className="text-xs-center">{subtitle}</p>}
            {children}
          </div>
          {sidebar && <div className="col-md-3">{sidebar}</div>}
        </div>
      );
    }

    return (
      <div className="row">
        <div className={columnClasses[columnLayout]}>
          {title && <h1 className="text-xs-center">{title}</h1>}
          {subtitle && <p className="text-xs-center">{subtitle}</p>}
          {children}
        </div>
      </div>
    );
  };

  return (
    <div className={className} data-testid="page-shell">
      {banner}
      <div className="container page">
        {renderContent()}
      </div>
    </div>
  );
};

