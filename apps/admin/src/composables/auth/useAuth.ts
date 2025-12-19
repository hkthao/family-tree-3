import { computed } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { FamilyRole } from '@/types/enums';
import { checkUserHasRole, checkUserHasFamilyRole } from './auth.logic';

interface UseAuthDeps {
  authStore: ReturnType<typeof useAuthStore>;
}

const defaultUseAuthDeps: UseAuthDeps = {
  get authStore() {
    return useAuthStore();
  },
};

export function useAuth(deps: UseAuthDeps = defaultUseAuthDeps) {
  const { authStore } = deps;

  const isLoggedIn = computed(() => authStore.isAuthenticated);
  const userRoles = computed(() => authStore.user?.roles || []);
  const currentUser = computed(() => authStore.user);
  const userFamilyAccess = computed(() => authStore.userFamilyAccess);

  const hasRole = (roles: string | string[]): boolean => {
    return checkUserHasRole(authStore.user, roles);
  };

  const isAdmin = computed(() => hasRole('Admin'));
  
  const hasFamilyRole = (familyId: string, role: FamilyRole): boolean => {
    return checkUserHasFamilyRole(authStore.userFamilyAccess, familyId, role);
  };

  const isFamilyManager = computed(() => (familyId: string): boolean => {
    return hasFamilyRole(familyId, FamilyRole.Manager);
  });

  return {
    state: {
      isLoggedIn,
      userRoles,
      isAdmin,
      isFamilyManager,
      currentUser,
      userFamilyAccess,
    },
    actions: {
      hasRole,
      hasFamilyRole,
    },
  };
}

