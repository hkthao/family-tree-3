import type { App } from 'vue';
import type { PiniaPluginContext } from 'pinia';
import { MockFamilyService, ApiFamilyService, IFamilyService } from '@/services';

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: {
      family: IFamilyService;
      // Add other services here as they are created
    };
  }
}

export function ServicesPlugin({ store }: PiniaPluginContext) {
  const isMockApi = import.meta.env.VITE_APP_USE_MOCK_API === 'true';

  const familyService: IFamilyService = isMockApi
    ? new MockFamilyService()
    : new ApiFamilyService();

  // Inject services into the store
  Object.defineProperty(store, 'services', {
    value: {
      family: familyService,
      // Add other services here
    },
    writable: false,
    configurable: false,
  });
}

// This is for global registration in main.ts
export default {
  install(app: App) {
    // No app-level installation needed for Pinia plugins, they are passed directly to Pinia
  },
};
