import { computed } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { FamilyRole } from '@/types/enums';

export function useAuth() {
  const authStore = useAuthStore();

  const isLoggedIn = computed(() => authStore.isAuthenticated);
  const userRoles = computed(() => authStore.user?.roles || []);

  const hasRole = (roles: string | string[]): boolean => {
    if (!authStore.isAuthenticated) {
      return false;
    }
    const rolesToCheck = Array.isArray(roles) ? roles : [roles];
    return rolesToCheck.some(role => userRoles.value.includes(role));
  };

  const isAdmin = computed(() => hasRole('Admin'));

  
  const hasFamilyRole = (familyId: string, role: FamilyRole): boolean => {
    if (!authStore.isAuthenticated) {
      return false;
    }
    return authStore.userFamilyAccess.some(
      access => access.familyId === familyId && access.role === role
    );
  };

  
  const isFamilyManager = computed(() => (familyId: string): boolean => {
    return hasFamilyRole(familyId, FamilyRole.Manager);
  });

  return {
    isLoggedIn,
    userRoles,
    hasRole,
    isAdmin,
    isFamilyManager,
    hasFamilyRole, 
  };
}

