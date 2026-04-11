import React from 'react';
import { Grid, Column } from '@carbon/react';
import './PageShell.css';

type ColumnLayout = 'narrow' | 'wide' | 'full' | 'two-column';

interface PageShellProps {
  children: React.ReactNode;
  className?: string;
  columnLayout?: ColumnLayout;
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  banner?: React.ReactNode;
  sidebar?: React.ReactNode;
}

const columnProps: Record<ColumnLayout, object> = {
  narrow: { lg: { span: 8, offset: 4 }, md: { span: 6, offset: 1 }, sm: 4 },
  wide: { lg: { span: 14, offset: 1 }, md: 8, sm: 4 },
  full: { lg: 16, md: 8, sm: 4 },
  'two-column': { lg: 11, md: 8, sm: 4 },
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

  return (
    <div className={pageClassName}>
      {banner && <div className="page-shell-banner">{banner}</div>}
      <Grid className="page-shell-grid">
        <Column {...columnProps[columnLayout]}>
          {title && <h1 className="page-shell-title">{title}</h1>}
          {subtitle && <p className="page-shell-subtitle">{subtitle}</p>}
          {children}
        </Column>
        {columnLayout === 'two-column' && sidebar && (
          <Column lg={5} md={8} sm={4}>{sidebar}</Column>
        )}
      </Grid>
    </div>
  );
};
