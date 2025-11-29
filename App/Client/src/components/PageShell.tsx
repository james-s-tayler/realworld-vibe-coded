import React from 'react';

type ColumnLayout = 'narrow' | 'wide';

interface PageShellProps {
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  children: React.ReactNode;
  className?: string; // e.g. "auth-page", "editor-page", ...
  columnLayout?: ColumnLayout; // 'narrow' (col-md-6) or 'wide' (col-md-10), default is 'narrow'
}

const columnClasses: Record<ColumnLayout, string> = {
  narrow: 'col-md-6 offset-md-3 col-xs-12',
  wide: 'col-md-10 offset-md-1 col-xs-12',
};

export const PageShell: React.FC<PageShellProps> = ({
  title,
  subtitle,
  children,
  className,
  columnLayout = 'narrow',
}) => (
  <div className={className} data-testid="page-shell">
    <div className="container page">
      <div className="row">
        <div className={columnClasses[columnLayout]}>
          {title && <h1 className="text-xs-center">{title}</h1>}
          {subtitle && <p className="text-xs-center">{subtitle}</p>}
          {children}
        </div>
      </div>
    </div>
  </div>
);
