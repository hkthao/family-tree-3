import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useAuth } from '@/composables/auth/useAuth';
import { useAuthStore } from '@/stores/auth.store';
import { FamilyRole } from '@/types/enums';
import { createPinia, setActivePinia } from 'pinia';
import { computed } from 'vue';

// Mock the auth store
vi.mock('@/stores/auth.store', () => ({
  useAuthStore: vi.fn(),
}));

describe('useAuth', () => {
  let mockAuthStore: ReturnType<typeof useAuthStore>;

  beforeEach(() => {
    setActivePinia(createPinia());
    mockAuthStore = {
      isAuthenticated: false,
      user: null,
      userFamilyAccess: [],
    } as ReturnType<typeof useAuthStore>;

    // Mock the useAuthStore to return our mock store
    (useAuthStore as vi.Mock).mockReturnValue(mockAuthStore);
  });

  // Test isLoggedIn
  it('isLoggedIn should return true when authenticated', () => {
    mockAuthStore.isAuthenticated = true;
    const { isLoggedIn } = useAuth();
    expect(isLoggedIn.value).toBe(true);
  });

  it('isLoggedIn should return false when not authenticated', () => {
    mockAuthStore.isAuthenticated = false;
    const { isLoggedIn } = useAuth();
    expect(isLoggedIn.value).toBe(false);
  });

  // Test userRoles
  it('userRoles should return an empty array if user is null', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = null;
    const { userRoles } = useAuth();
    expect(userRoles.value).toEqual([]);
  });

  it('userRoles should return user roles if user exists', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { roles: ['User', 'Editor'] } as any;
    const { userRoles } = useAuth();
    expect(userRoles.value).toEqual(['User', 'Editor']);
  });

  // Test hasRole
  it('hasRole should return false if not authenticated', () => {
    mockAuthStore.isAuthenticated = false;
    mockAuthStore.user = { roles: ['User'] } as any;
    const { hasRole } = useAuth();
    expect(hasRole('User')).toBe(false);
  });

  it('hasRole should return true for a single role that the user has', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { roles: ['User', 'Editor'] } as any;
    const { hasRole } = useAuth();
    expect(hasRole('Editor')).toBe(true);
  });

  it('hasRole should return false for a single role that the user does not have', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { roles: ['User'] } as any;
    const { hasRole } = useAuth();
    expect(hasRole('Admin')).toBe(false);
  });

  it('hasRole should return true for multiple roles if the user has at least one', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { roles: ['User', 'Viewer'] } as any;
    const { hasRole } = useAuth();
    expect(hasRole(['Admin', 'Viewer'])).toBe(true);
  });

  it('hasRole should return false for multiple roles if the user has none of them', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { roles: ['User'] } as any;
    const { hasRole } = useAuth();
    expect(hasRole(['Admin', 'Moderator'])).toBe(false);
  });

  // Test isAdmin
  it('isAdmin should return true if user has Admin role', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { roles: ['User', 'Admin'] } as any;
    const { isAdmin } = useAuth();
    expect(isAdmin.value).toBe(true);
  });

  it('isAdmin should return false if user does not have Admin role', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.user = { roles: ['User'] } as any;
    const { isAdmin } = useAuth();
    expect(isAdmin.value).toBe(false);
  });

  // Test hasFamilyRole
  it('hasFamilyRole should return false if not authenticated', () => {
    mockAuthStore.isAuthenticated = false;
    mockAuthStore.userFamilyAccess = [{ familyId: 'family1', role: FamilyRole.Manager }];
    const { hasFamilyRole } = useAuth();
    expect(hasFamilyRole('family1', FamilyRole.Manager)).toBe(false);
  });

  it('hasFamilyRole should return true if user has the specified family role', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.userFamilyAccess = [{ familyId: 'family1', role: FamilyRole.Manager }];
    const { hasFamilyRole } = useAuth();
    expect(hasFamilyRole('family1', FamilyRole.Manager)).toBe(true);
  });

  it('hasFamilyRole should return false if user does not have the specified family role for the family', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.userFamilyAccess = [{ familyId: 'family1', role: FamilyRole.Member }];
    const { hasFamilyRole } = useAuth();
    expect(hasFamilyRole('family1', FamilyRole.Manager)).toBe(false);
  });

  it('hasFamilyRole should return false if user does not have access to the family', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.userFamilyAccess = [{ familyId: 'family2', role: FamilyRole.Manager }];
    const { hasFamilyRole } = useAuth();
    expect(hasFamilyRole('family1', FamilyRole.Manager)).toBe(false);
  });

  // Test isFamilyManager
  it('isFamilyManager should return true if user is manager for the given familyId', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.userFamilyAccess = [{ familyId: 'familyA', role: FamilyRole.Manager }];
    const { isFamilyManager } = useAuth();
    expect(isFamilyManager.value('familyA')).toBe(true);
  });

  it('isFamilyManager should return false if user is not manager for the given familyId', () => {
    mockAuthStore.isAuthenticated = true;
    mockAuthStore.userFamilyAccess = [
      { familyId: 'familyA', role: FamilyRole.Member },
      { familyId: 'familyB', role: FamilyRole.Manager },
    ];
    const { isFamilyManager } = useAuth();
    expect(isFamilyManager.value('familyA')).toBe(false);
    expect(isFamilyManager.value('familyC')).toBe(false); // No access to familyC
  });
});