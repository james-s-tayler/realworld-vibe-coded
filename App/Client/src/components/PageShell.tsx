import React from 'react';
import { Grid, Column } from '@carbon/react';
import './PageShell.css';

type ColumnLayout = 'narrow' | 'wide' | 'full' | 'two-column';

interface PageShellProps {
  /** Main page content */
  children: React.ReactNode;
  /** Page-specific class name (e.g. "auth-page", "editor-page", "home-page") */
  className?: string;
  /** Column layout: 'narrow' (centered), 'wide' (10/16), 'full' (16/16), 'two-column' (10/16 + sidebar) */
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

const columnSpans: Record<ColumnLayout, { sm: number; md: number; lg: number | { span: number; offset: number } }> = {
  narrow: { sm: 4, md: 6, lg: { span: 8, offset: 4 } },
  wide: { sm: 4, md: 8, lg: { span: 12, offset: 2 } },
  full: { sm: 4, md: 8, lg: 16 },
  'two-column': { sm: 4, md: 8, lg: 11 },
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
  const span = columnSpans[columnLayout];

  return (
    <div className={pageClassName}>
      {banner && <div className="page-shell-banner">{banner}</div>}
      <Grid narrow>
        <Column sm={span.sm} md={span.md} lg={span.lg}>
          {title && <h1 className="page-shell-title">{title}</h1>}
          {subtitle && <p className="page-shell-subtitle">{subtitle}</p>}
          {children}
        </Column>
        {columnLayout === 'two-column' && sidebar && (
          <Column sm={4} md={8} lg={5}>
            {sidebar}
          </Column>
        )}
      </Grid>
    </div>
  );
};
