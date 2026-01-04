import React from 'react';
import { useHasRole } from '../hooks/useHasRole';

interface RequireRoleProps {
  roles: string[];
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

/**
 * Component that conditionally renders children based on user roles
 * @param roles - Array of role names required to render children
 * @param children - Content to render if user has required role
 * @param fallback - Optional content to render if user doesn't have required role
 */
export const RequireRole: React.FC<RequireRoleProps> = ({ roles, children, fallback = null }) => {
  const hasRole = useHasRole(roles);
  
  return <>{hasRole ? children : fallback}</>;
};
