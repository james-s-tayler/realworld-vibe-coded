import React from 'react';
import { Link } from 'react-router';
import { Button } from '@carbon/react';
import { Settings, UserAvatar } from '@carbon/icons-react';
import { useAuth } from '../hooks/useAuth';
import { PageShell } from '../components/PageShell';

export const DashboardPage: React.FC = () => {
  const { user } = useAuth();

  return (
    <PageShell className="dashboard-page">
      <h1>Welcome{user ? `, ${user.username}` : ''}!</h1>
      <p>This is your dashboard. Get started by exploring:</p>
      <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
        <Link to="/settings">
          <Button kind="primary" renderIcon={Settings}>
            Settings
          </Button>
        </Link>
        {user && (
          <Link to={`/profile/${user.username}`}>
            <Button kind="secondary" renderIcon={UserAvatar}>
              View Profile
            </Button>
          </Link>
        )}
      </div>
    </PageShell>
  );
};
