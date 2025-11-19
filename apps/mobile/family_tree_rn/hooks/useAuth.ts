import { useState, useEffect } from 'react';

interface User {
  fullName: string;
  email: string;
  phoneNumber?: string;
  avatarUrl?: string;
}

export const useAuth = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userRoles, setUserRoles] = useState<string[]>([]);
  const [user, setUser] = useState<User | null>(null); // Add user state

  useEffect(() => {
    // Placeholder for actual authentication logic
    const checkAuthStatus = async () => {
      await new Promise(resolve => setTimeout(resolve, 100));
      // Simulate a logged-in user for development
      setIsLoggedIn(true);
      setUserRoles(['User']);
      setUser({
        fullName: 'John Doe',
        email: 'john.doe@example.com',
        avatarUrl: 'https://picsum.photos/seed/johndoe/150/150',
      });
    };

    checkAuthStatus();
  }, []);

  const login = async () => {
    // Simulate login
    await new Promise(resolve => setTimeout(resolve, 500));
    setIsLoggedIn(true);
    setUserRoles(['User']);
    setUser({
      fullName: 'John Doe',
      email: 'john.doe@example.com',
      avatarUrl: 'https://picsum.photos/seed/johndoe/150/150',
    });
  };

  const logout = async () => {
    // Simulate logout
    await new Promise(resolve => setTimeout(resolve, 500));
    setIsLoggedIn(false);
    setUserRoles([]);
    setUser(null);
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
    user, // Return user object
    userRoles,
    hasRole,
    isAdmin,
    isFamilyManager,
    login, // Return login function
    logout, // Return logout function
  };
};
