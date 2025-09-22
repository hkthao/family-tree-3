import type { App } from 'vue';
import type { PiniaPluginContext } from 'pinia';
import { createServices, ServiceMode, AppServices } from '@/services/service.factory'; // Import the factory
import type { IFamilyService, IMemberService } from '@/services'; // Use import type for interfaces

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: AppServices; // Use AppServices from the factory
  }
}

export function ServicesPlugin() {
  return ({ store }: PiniaPluginContext) => {
    const isMockApi = import.meta.env.VITE_APP_USE_MOCK_API === 'true';
    const mode: ServiceMode = isMockApi ? 'mock' : 'real';
    const services = createServices(mode);

    // Inject services into the store
    Object.defineProperty(store, 'services', {
      value: services,
      writable: false,
      configurable: false,
    });
  };
}

// This is for global registration in main.ts
export default {
  install(app: App) {
    // No app-level installation needed for Pinia plugins, they are passed directly to Pinia
  },
};

