import React from 'react';

interface PageShellProps {
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  children: React.ReactNode;
  className?: string; // e.g. "auth-page", "editor-page", ...
}

export const PageShell: React.FC<PageShellProps> = ({
  title,
  subtitle,
  children,
  className,
}) => (
  <div className={className} data-testid="page-shell">
    <div className="container page">
      <div className="row">
        <div className="col-md-6 offset-md-3 col-xs-12">
          {title && <h1 className="text-xs-center">{title}</h1>}
          {subtitle && <p className="text-xs-center">{subtitle}</p>}
          {children}
        </div>
      </div>
    </div>
  </div>
);
