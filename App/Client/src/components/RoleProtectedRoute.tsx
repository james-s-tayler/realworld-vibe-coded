import React from 'react';
import { Navigate } from 'react-router';
import { useAuth } from '../hooks/useAuth';
import { useHasRole } from '../hooks/useHasRole';
import { Loading } from '@carbon/react';

interface RoleProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles: string[];
}

export const RoleProtectedRoute: React.FC<RoleProtectedRouteProps> = ({ children, requiredRoles }) => {
  const { user, loading } = useAuth();
  const hasRequiredRole = useHasRole(requiredRoles);

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <Loading description="Loading..." withOverlay={false} />
      </div>
    );
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }
  
  if (!hasRequiredRole) {
    return <Navigate to="/forbidden" replace />;
  }

  return <>{children}</>;
};
