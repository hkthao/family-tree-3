import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useAuth } from '@/composables/auth/useAuth';
import { createPinia, setActivePinia } from 'pinia';
import { FamilyRole } from '@/types/enums';
import type { UserProfile } from '@/types';
import type { IFamilyAccess } from '@/types/family.d';
import { checkUserHasRole, checkUserHasFamilyRole } from '@/composables/auth/auth.logic'; // Import the actual functions

// Mock the auth store
const mockAuthStore = {
  isAuthenticated: false,
  user: null as UserProfile | null,
  userFamilyAccess: [] as IFamilyAccess[],
};

// Mock the @/stores/auth.store module
vi.mock('@/stores/auth.store', () => ({
  useAuthStore: vi.fn(() => mockAuthStore),
}));

// Mock the auth.logic module, as useAuth now depends on it
vi.mock('@/composables/auth/auth.logic', () => ({
  checkUserHasRole: vi.fn((userProfile, roles) => {
    if (!userProfile?.roles) {
      return false;
    }
    const rolesToCheck = Array.isArray(roles) ? roles : [roles];
    return rolesToCheck.some(role => userProfile.roles?.includes(role));
  }),
  checkUserHasFamilyRole: vi.fn((userFamilyAccess, familyId, role) => {
    if (!userFamilyAccess || userFamilyAccess.length === 0) {
      return false;
    }
    return userFamilyAccess.some(access => access.familyId === familyId && access.role === role);
  }),
}));

describe('useAuth', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    // Reset mockAuthStore before each test
    mockAuthStore.isAuthenticated = false;
    mockAuthStore.user = null;
    mockAuthStore.userFamilyAccess = [];
    vi.clearAllMocks();
    // Reset mocks for auth.logic functions as well
    vi.mocked(checkUserHasRole).mockClear();
    vi.mocked(checkUserHasFamilyRole).mockClear();
  });

  it('should return isLoggedIn as false when not authenticated', () => {
    const { state } = useAuth();
    expect(state.isLoggedIn.value).toBe(false);
  });

  it('should return isLoggedIn as true when authenticated', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'test@example.com', name: 'Test User', roles: ['User'] };
    const { state } = useAuth();
    expect(state.isLoggedIn.value).toBe(true);
  });

  it('should return correct user roles', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'test@example.com', name: 'Test User', roles: ['User', 'Editor'] };
    const { state } = useAuth();
    expect(state.userRoles.value).toEqual(['User', 'Editor']);
  });

  it('should return an empty array for user roles if user is null', () => {
    const { state } = useAuth();
    expect(state.userRoles.value).toEqual([]);
  });

  it('should correctly determine if user has a specific role', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'test@example.com', name: 'Test User', roles: ['User', 'Admin'] };
    const { actions } = useAuth();
    expect(actions.hasRole('Admin')).toBe(true);
    expect(actions.hasRole('User')).toBe(true);
    expect(actions.hasRole('Guest')).toBe(false);
  });

  it('should correctly determine if user has one of multiple roles', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'test@example.com', name: 'Test User', roles: ['User'] };
    const { actions } = useAuth();
    expect(actions.hasRole(['User', 'Admin'])).toBe(true);
    expect(actions.hasRole(['Guest', 'Editor'])).toBe(false);
  });

  it('should return isAdmin as true for an Admin user', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'admin@example.com', name: 'Admin User', roles: ['Admin'] };
    const { state } = useAuth();
    expect(state.isAdmin.value).toBe(true);
  });

  it('should return isAdmin as false for a non-Admin user', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'user@example.com', name: 'User', roles: ['User'] };
    const { state } = useAuth();
    expect(state.isAdmin.value).toBe(false);
  });

  it('should correctly determine if user has a specific family role', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'test@example.com', name: 'Test User', roles: ['User'] };
    mockAuthStore.userFamilyAccess = [{ familyId: 'family123', role: FamilyRole.Manager }];
    const { actions } = useAuth();
    expect(actions.hasFamilyRole('family123', FamilyRole.Manager)).toBe(true);
    expect(actions.hasFamilyRole('family123', FamilyRole.Member)).toBe(false);
    expect(actions.hasFamilyRole('family456', FamilyRole.Manager)).toBe(false);
  });

  it('should return isFamilyManager as true for a family manager', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'test@example.com', name: 'Test User', roles: ['User'] };
    mockAuthStore.userFamilyAccess = [{ familyId: 'family123', role: FamilyRole.Manager }];
    const { state } = useAuth();
    expect(state.isFamilyManager.value('family123')).toBe(true);
  });

  it('should return isFamilyManager as false for a non-family manager', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { id: '1', email: 'test@example.com', name: 'Test User', roles: ['User'] };
    mockAuthStore.userFamilyAccess = [{ familyId: 'family123', role: FamilyRole.Member }];
    const { state } = useAuth();
    expect(state.isFamilyManager.value('family123')).toBe(false);
  });

  it('should expose currentUser and userFamilyAccess from the store', () => {
    mockAuthStore.isAuthenticated = true;
    const userProfile: UserProfile = { id: 'user1', email: 'user@example.com', name: 'User One', roles: ['User'] };
    const familyAccess: IFamilyAccess[] = [{ familyId: 'fam1', role: FamilyRole.Member }];
    mockAuthStore.user = userProfile;
    mockAuthStore.userFamilyAccess = familyAccess;

    const { state } = useAuth();

    expect(state.currentUser.value).toEqual(userProfile);
    expect(state.userFamilyAccess.value).toEqual(familyAccess);
  });
});

