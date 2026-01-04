import React from 'react';
import { Link } from 'react-router';
import { PageShell } from '../components/PageShell';
import './ForbiddenPage.css';

export const ForbiddenPage: React.FC = () => {
  return (
    <PageShell className="forbidden-page">
      <h1>403 - Forbidden</h1>
      <p>You don't have permission to access this page.</p>
      <p>
        <Link to="/">Go back to home</Link>
      </p>
    </PageShell>
  );
};
