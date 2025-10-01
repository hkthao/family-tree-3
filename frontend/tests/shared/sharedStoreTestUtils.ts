
import { createPinia, setActivePinia } from 'pinia';
import { createServices } from '@/services/service.factory';

/**
 * Initializes and prepares a Pinia store for testing with a mocked service.
 *
 * @param useStore - The store hook (e.g., useFamilyStore) to get the store instance.
 * @param serviceName - The key of the service to mock (e.g., 'family', 'member').
 * @param mockService - The mock service instance to inject into the store.
 * @returns The fully initialized store instance ready for testing.
 */
export function setupStoreForTesting<T extends () => any>(
  useStore: T,
  serviceName: string,
  mockService: any,
) {
  setActivePinia(createPinia());
  const store = useStore();
  store.services = createServices('test', { [serviceName]: mockService });
  return store;
}
