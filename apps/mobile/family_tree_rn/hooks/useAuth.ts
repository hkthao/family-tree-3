import { useState, useEffect } from 'react';
import { authService } from '@/services/authService'; // Import authService

interface User {
  fullName: string;
  email: string;
  phoneNumber?: string;
  avatarUrl?: string;
  sub: string; // Add sub for Auth0 user ID
}

export const useAuth = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userRoles, setUserRoles] = useState<string[]>([]);
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    const checkAuthStatus = async () => {
      const authenticated = authService.isAuthenticated();
      setIsLoggedIn(authenticated);
      if (authenticated) {
        const auth0User = authService.getUser();
        if (auth0User) {
          setUser({
            fullName: auth0User.name,
            email: auth0User.email,
            avatarUrl: auth0User.picture,
            sub: auth0User.sub,
          });
          // TODO: Fetch user roles from a backend API based on auth0User.sub
          setUserRoles(['User']); // Default role for now
        }
      } else {
        setUser(null);
        setUserRoles([]);
      }
    };

    checkAuthStatus();
  }, []);

  const login = async () => {
    const success = await authService.login();
    if (success) {
      const auth0User = authService.getUser();
      if (auth0User) {
        setIsLoggedIn(true);
        setUser({
          fullName: auth0User.name,
          email: auth0User.email,
          avatarUrl: auth0User.picture,
          sub: auth0User.sub,
        });
        // TODO: Fetch user roles from a backend API
        setUserRoles(['User']); // Default role for now
      }
    }
    return success;
  };

  const logout = async () => {
    await authService.logout();
    setIsLoggedIn(false);
    setUser(null);
    setUserRoles([]);
  };

  const hasRole = (roles: string | string[]): boolean => {
    if (!isLoggedIn) {
      return false;
    }
    const rolesToCheck = Array.isArray(roles) ? roles : [roles];
    return rolesToCheck.some(role => userRoles.includes(role));
  };

  const isAdmin = hasRole('Admin');
  const isFamilyManager = hasRole('FamilyManager');

  return {
    isLoggedIn,
    user,
    userRoles,
    hasRole,
    isAdmin,
    isFamilyManager,
    login,
    logout,
  };
};