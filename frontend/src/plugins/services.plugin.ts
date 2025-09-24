import type { App } from 'vue';
import type { PiniaPluginContext } from 'pinia';
import { createServices, type ServiceMode, type AppServices } from '@/services/service.factory'; // Import the factory

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: AppServices;
  }
}

export function ServicesPlugin() {
  return ({ store }: PiniaPluginContext) => {
    const isMockApi = import.meta.env.VITE_USE_MOCK === 'true';
    const mode: ServiceMode = isMockApi ? 'mock' : 'real';
    store.services= createServices(mode);
  };
}

// This is for global registration in main.ts
export default {
  install(app: App) {
    // No app-level installation needed for Pinia plugins, they are passed directly to Pinia
  },
};

