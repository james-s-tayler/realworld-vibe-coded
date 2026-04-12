import './PageShell.scss';

import { Column,Grid } from '@carbon/react';
import React from 'react';

type ColumnLayout = 'narrow' | 'wide' | 'full' | 'two-column';

interface PageShellProps {
  children: React.ReactNode;
  className?: string;
  columnLayout?: ColumnLayout;
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  banner?: React.ReactNode;
  breadcrumbs?: React.ReactNode;
  sidebar?: React.ReactNode;
}

const columnProps: Record<ColumnLayout, object> = {
  narrow: { lg: 8, md: 6, sm: 4 },
  wide: { lg: 14, md: 8, sm: 4 },
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
  breadcrumbs,
  sidebar,
}) => {
  return (
    <div className={['page-shell', className].filter(Boolean).join(' ')}>
      {banner}
      <Grid>
        <Column {...columnProps[columnLayout]}>
          {breadcrumbs}
          {(title || subtitle) && (
            <div className="page-shell__header">
              {title && <h1>{title}</h1>}
              {subtitle && <p>{subtitle}</p>}
            </div>
          )}
          {children}
        </Column>
        {columnLayout === 'two-column' && sidebar && (
          <Column lg={5} md={8} sm={4}>{sidebar}</Column>
        )}
      </Grid>
    </div>
  );
};
