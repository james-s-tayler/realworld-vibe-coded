import { useAuth } from './useAuth';

/**
 * Hook to check if the current user has any of the specified roles
 * @param requiredRoles - Array of role names to check
 * @returns true if user has at least one of the required roles, false otherwise
 */
export const useHasRole = (requiredRoles: string[]): boolean => {
  const { user } = useAuth();
  
  if (!user || !user.roles) {
    return false;
  }
  
  return requiredRoles.some(role => user.roles.includes(role));
};
