import { useAuthStore } from '@/stores/auth.store'; // Assuming you have a main store
import type { AppServices } from '@/services/service.factory';

export function useServices(): AppServices {
  const authStore = useAuthStore(); // Get your main Pinia store instance
  return authStore.services;
}
