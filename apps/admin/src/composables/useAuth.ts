import { computed } from 'vue';
import { useAuthStore } from '@/stores/auth.store';

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
  const isFamilyManager = computed(() => hasRole('FamilyManager'));

  // You can add more specific permission checks here if needed
  // For example, if you have a 'permissions' array in your user object:
  // const can = (permission: string | string[]): boolean => {
  //   if (!authStore.isAuthenticated) {
  //     return false;
  //   }
  //   const permissionsToCheck = Array.isArray(permission) ? permission : [permission];
  //   return permissionsToCheck.some(p => authStore.user?.permissions?.includes(p));
  // };

  return {
    isLoggedIn,
    userRoles,
    hasRole,
    isAdmin,
    isFamilyManager,
    // can, // Uncomment if you implement a 'can' function
  };
}
