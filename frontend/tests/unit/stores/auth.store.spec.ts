import { setActivePinia, createPinia } from 'pinia';
import { useAuthStore } from '@/stores/auth.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { User } from '@/types';

// Mock the authService
const mockGetUser = vi.fn();
const mockGetAccessToken = vi.fn();
const mockLogin = vi.fn();
const mockLogout = vi.fn();
const mockRegister = vi.fn();

vi.mock('@/services/auth/authService', () => ({
  useAuthService: () => ({
    getUser: mockGetUser,
    getAccessToken: mockGetAccessToken,
    login: mockLogin,
    logout: mockLogout,
    register: mockRegister,
  }),
}));

describe('auth.store', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    // Reset mocks before each test
    mockGetUser.mockReset();
    mockGetAccessToken.mockReset();
    mockLogin.mockReset();
    mockLogout.mockReset();
    mockRegister.mockReset();
  });

  // --- Getters Tests ---
  it('isAuthenticated should return true if user is present', () => {
    const store = useAuthStore();
    store.user = {
      id: '1',
      name: 'test',
      email: 'test@example.com',
      roles: [],
    } as User;
    expect(store.isAuthenticated).toBe(true);
  });

  it('isAuthenticated should return false if user is null', () => {
    const store = useAuthStore();
    store.user = null;
    expect(store.isAuthenticated).toBe(false);
  });

  it('isAdmin should return true if user has Admin role', () => {
    const store = useAuthStore();
    store.user = {
      id: '1',
      email: 'test@example.com',
      roles: ['Admin'],
    } as User;
    expect(store.isAdmin).toBe(true);
  });

  it('isAdmin should return false if user does not have Admin role', () => {
    const store = useAuthStore();
    store.user = {
      id: '1',
      email: 'test@example.com',
      roles: ['User'],
    } as User;
    expect(store.isAdmin).toBe(false);
  });

  it('isFamilyManager should return true if user has FamilyManager role', () => {
    const store = useAuthStore();
    store.user = {
      id: '1',
      email: 'test@example.com',
      roles: ['FamilyManager'],
    } as User;
    expect(store.isFamilyManager).toBe(true);
  });

  it('isFamilyManager should return false if user does not have FamilyManager role', () => {
    const store = useAuthStore();
    store.user = {
      id: '1',
      email: 'test@example.com',
      roles: ['User'],
    } as User;
    expect(store.isFamilyManager).toBe(false);
  });

  // --- Actions Tests ---

  describe('initAuth', () => {
    it('should initialize auth successfully', async () => {
      const store = useAuthStore();
      const mockUser = {
        id: '1',
        email: 'test@example.com',
        name: 'test',
        roles: [],
      } as User;
      const mockToken = 'mock-token';

      mockGetUser.mockResolvedValue(mockUser);
      mockGetAccessToken.mockResolvedValue(mockToken);

      await store.initAuth();

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.user).toEqual(mockUser);
      expect(store.token).toBe(mockToken);
      expect(mockGetUser).toHaveBeenCalledTimes(1);
      expect(mockGetAccessToken).toHaveBeenCalledTimes(1);
    });

    it('should handle initAuth failure', async () => {
      const store = useAuthStore();
      const errorMessage = 'Auth init failed';

      mockGetUser.mockRejectedValue(new Error(errorMessage));

      await store.initAuth();

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockGetUser).toHaveBeenCalledTimes(1);
      expect(mockGetAccessToken).not.toHaveBeenCalled();
    });
  });

  describe('login', () => {
    const credentials = { email: 'test@example.com', password: 'password' };

    it('should login successfully', async () => {
      const store = useAuthStore();
      const mockUser = {
        id: '1',
        email: 'test@example.com',
        name: 'test',
        roles: [],
      } as User;
      const mockToken = 'mock-token';

      mockLogin.mockResolvedValue(mockUser);
      mockGetAccessToken.mockResolvedValue(mockToken);

      await store.login(credentials);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.user).toEqual(mockUser);
      expect(store.token).toBe(mockToken);
      expect(mockLogin).toHaveBeenCalledWith(credentials);
      expect(mockGetAccessToken).toHaveBeenCalledTimes(1);
    });

    it('should handle login failure (invalid credentials)', async () => {
      const store = useAuthStore();

      mockLogin.mockResolvedValue(null);
      mockGetAccessToken.mockResolvedValue(null);

      await store.login(credentials);

      expect(store.loading).toBe(false);
      expect(store.error).toBe('Invalid credentials or login failed.');
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockLogin).toHaveBeenCalledWith(credentials);
    });

    it('should handle login failure (service error)', async () => {
      const store = useAuthStore();
      const errorMessage = 'Network error';

      mockLogin.mockRejectedValue(new Error(errorMessage));

      await store.login(credentials);

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockLogin).toHaveBeenCalledWith(credentials);
    });
  });

  describe('logout', () => {
    it('should logout successfully', async () => {
      const store = useAuthStore();
      store.user = {
        id: '1',
        name: 'test',
        email: 'test@example.com',
        roles: [],
      } as User;
      store.token = 'existing-token';

      mockLogout.mockResolvedValue(undefined);

      await store.logout();

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockLogout).toHaveBeenCalledTimes(1);
    });

    it('should handle logout failure', async () => {
      const store = useAuthStore();
      store.user = {
        id: '1',
        name: 'test',
        email: 'test@example.com',
        roles: [],
      } as User;
      store.token = 'existing-token';
      const errorMessage = 'Logout service error';

      mockLogout.mockRejectedValue(new Error(errorMessage));

      await store.logout();

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      // User and token should remain if logout failed to clear them
      expect(store.user).toEqual({
        id: '1',
        name: 'test',
        email: 'test@example.com',
        roles: [],
      });
      expect(store.token).toBe('existing-token');
      expect(mockLogout).toHaveBeenCalledTimes(1);
    });
  });

  describe('register', () => {
    const registrationData = {
      email: 'new@example.com',
      password: 'newpassword',
      name: 'New User',
    };

    it('should register successfully', async () => {
      const store = useAuthStore();
      const mockUser = {
        id: '2',
        name: 'test',
        email: 'new@example.com',
        roles: [],
      } as User;
      const mockToken = 'new-mock-token';

      mockRegister.mockResolvedValue(mockUser);
      mockGetAccessToken.mockResolvedValue(mockToken);

      await store.register(registrationData);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.user).toEqual(mockUser);
      expect(store.token).toBe(mockToken);
      expect(mockRegister).toHaveBeenCalledWith(registrationData);
      expect(mockGetAccessToken).toHaveBeenCalledTimes(1);
    });

    it('should handle registration failure (service returns null user)', async () => {
      const store = useAuthStore();

      mockRegister.mockResolvedValue(null);
      mockGetAccessToken.mockResolvedValue(null);

      await store.register(registrationData);

      expect(store.loading).toBe(false);
      expect(store.error).toBe('Registration failed.');
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockRegister).toHaveBeenCalledWith(registrationData);
    });

    it('should handle registration failure (service error)', async () => {
      const store = useAuthStore();
      const errorMessage = 'Registration service error';

      mockRegister.mockRejectedValue(new Error(errorMessage));

      await store.register(registrationData);

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockRegister).toHaveBeenCalledWith(registrationData);
    });
  });
});
