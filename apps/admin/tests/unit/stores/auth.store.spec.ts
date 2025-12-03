import { setActivePinia, createPinia } from 'pinia';
import { useAuthStore } from '@/stores/auth.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { User } from '@/types';

let mockGetUser: ReturnType<typeof vi.fn>;
let mockGetAccessToken: ReturnType<typeof vi.fn>;
let mockLogin: ReturnType<typeof vi.fn>;
let mockLogout: ReturnType<typeof vi.fn>;
let mockRegister: ReturnType<typeof vi.fn>;
let mockGetFamilyAccess: ReturnType<typeof vi.fn>; // Declare this here as well

// Mock the useAuthService hook
vi.mock('@/services/auth/authService', () => ({
  useAuthService: vi.fn(() => ({
    getUser: mockGetUser,
    getAccessToken: mockGetAccessToken,
    login: mockLogin,
    logout: mockLogout,
    register: mockRegister,
  })),
}));

// Mock the entire service factory
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    ai: {},
    auth: {},
    chat: {},
    dashboard: {},
    event: {},
    face: {},
    faceMember: {},
    family: {
      getUserFamilyAccess: mockGetFamilyAccess,
    },
    fileUpload: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => {
        if (key === 'auth.registrationFailed') return 'Registration failed.';
        return key;
      }),
    },
  },
}));

// Mock vue-i18n globally for composition API usage
vi.mock('vue-i18n', async (importOriginal) => {
  const actual = await importOriginal();
  return {
    ...(actual as Record<string, unknown>),
    useI18n: () => ({
      t: vi.fn((key) => {
        if (key === 'auth.registrationFailed') return 'Registration failed.';
        return key;
      }),
    }),
  };
});

describe('auth.store', () => {
  let store: ReturnType<typeof useAuthStore>;

  const mockUser: User = {
    id: 'user-1',
    email: 'test@example.com',
    roles: ['User'],
    externalId: '',
    name: '',
    familyId: 'mockFamilyId', // Added familyId
  };
  const mockToken = 'mock-jwt-token';

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useAuthStore();
    store.$reset();

    store.services = { // Thêm mock services vào store
      family: {
        getUserFamilyAccess: mockGetFamilyAccess,
      } as any, // Ép kiểu để Vitest không báo lỗi type
    } as any;


    // Re-initialize mocks before each test
    mockGetUser = vi.fn();
    mockGetAccessToken = vi.fn();
    mockLogin = vi.fn();
    mockLogout = vi.fn();
    mockRegister = vi.fn();
    mockGetFamilyAccess = vi.fn();

    // Set default mock resolved values
    mockGetUser.mockResolvedValue(mockUser);
    mockGetAccessToken.mockResolvedValue(mockToken);
    mockLogin.mockResolvedValue(undefined);
    mockLogout.mockResolvedValue(undefined);
    mockRegister.mockResolvedValue(mockUser);
    mockGetFamilyAccess.mockResolvedValue({ ok: true, value: [] });
  });

  it('should have correct initial state', () => {
    expect(store.user).toBeNull();
    expect(store.token).toBeNull();
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('getters', () => {
    it('isAuthenticated should return true if user exists', () => {
      store.user = mockUser;
      expect(store.isAuthenticated).toBe(true);
    });

    it('isAuthenticated should return false if user is null', () => {
      store.user = null;
      expect(store.isAuthenticated).toBe(false);
    });

    it('isAdmin should return true if user has Admin role', () => {
      store.user = { ...mockUser, roles: ['User', 'Admin'] };
      expect(store.isAdmin).toBe(true);
    });

    it('isAdmin should return false if user does not have Admin role', () => {
      store.user = mockUser;
      expect(store.isAdmin).toBe(false);
    });

    it('isFamilyManager should return true if user has FamilyManager role', () => {
      store.user = { ...mockUser, roles: ['User', 'FamilyManager'] };
      expect(store.isFamilyManager).toBe(true);
    });

    it('isFamilyManager should return false if user does not have FamilyManager role', () => {
      store.user = mockUser;
      expect(store.isFamilyManager).toBe(false);
    });

    it('getAccessToken should return the token', () => {
      store.token = mockToken;
      expect(store.getAccessToken).toBe(mockToken);
    });
  });

  describe('initAuth', () => {
    it('should initialize auth successfully', async () => {
      const result = await store.initAuth();

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.user).toEqual(mockUser);
      expect(store.token).toBe(mockToken);
      expect(mockGetUser).toHaveBeenCalledTimes(1);
      expect(mockGetAccessToken).toHaveBeenCalledTimes(1);
    });

    it('should handle init auth failure', async () => {
      const errorMessage = 'Failed to get user.';
      mockGetUser.mockRejectedValue(new Error(errorMessage));

      const result = await store.initAuth();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockGetUser).toHaveBeenCalledTimes(1);
      expect(mockGetAccessToken).not.toHaveBeenCalled(); // Should not be called if getUser fails
    });
  });

  describe('login', () => {
    it('should initiate login successfully', async () => {
      const result = await store.login();

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockLogin).toHaveBeenCalledTimes(1);
      expect(mockLogin).toHaveBeenCalledWith({ appState: { target: '/' } });
    });

    it('should handle login failure', async () => {
      const errorMessage = 'Login failed.';
      mockLogin.mockRejectedValue(new Error(errorMessage));

      const result = await store.login();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(mockLogin).toHaveBeenCalledTimes(1);
    });
  });

  describe('logout', () => {
    it('should logout successfully', async () => {
      store.user = mockUser;
      store.token = mockToken;

      const result = await store.logout();

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockLogout).toHaveBeenCalledTimes(1);
    });

    it('should handle logout failure', async () => {
      const errorMessage = 'Logout failed.';
      mockLogout.mockRejectedValue(new Error(errorMessage));

      store.user = mockUser;
      store.token = mockToken;

      const result = await store.logout();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.user).toEqual(mockUser); // State should not be cleared on failure
      expect(store.token).toBe(mockToken); // State should not be cleared on failure
      expect(mockLogout).toHaveBeenCalledTimes(1);
    });
  });

  describe('register', () => {
    it('should register successfully', async () => {
      const registerData = { email: 'new@example.com', password: 'password' };
      mockRegister.mockResolvedValue(mockUser);
      mockGetAccessToken.mockResolvedValue('new-token');

      const result = await store.register(registerData);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.user).toEqual(mockUser);
      expect(store.token).toBe('new-token');
      expect(mockRegister).toHaveBeenCalledTimes(1);
      expect(mockRegister).toHaveBeenCalledWith(registerData);
      expect(mockGetAccessToken).toHaveBeenCalledTimes(1);
    });

    it('should handle register failure (service error)', async () => {
      const errorMessage = 'Registration failed.';
      mockRegister.mockRejectedValue(new Error(errorMessage));
      const registerData = { email: 'new@example.com', password: 'password' };

      const result = await store.register(registerData);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockRegister).toHaveBeenCalledTimes(1);
      expect(mockGetAccessToken).not.toHaveBeenCalled();
    });

    it('should handle register failure (no user returned)', async () => {
      mockRegister.mockResolvedValue(null);
      mockGetAccessToken.mockResolvedValue(null); // Ensure token is null if no user
      const registerData = { email: 'new@example.com', password: 'password' };

      const result = await store.register(registerData);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('Đăng ký thất bại.'); // Changed to Vietnamese string
      expect(store.user).toBeNull();
      expect(store.token).toBeNull();
      expect(mockRegister).toHaveBeenCalledTimes(1);
      expect(mockGetAccessToken).toHaveBeenCalledTimes(1); // It is called, but should return null
    });
  });
});