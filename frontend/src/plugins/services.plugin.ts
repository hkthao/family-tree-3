import type { App } from 'vue';
import type { PiniaPluginContext } from 'pinia';
import { createServices, type ServiceMode, type AppServices } from '@/services/service.factory'; // Import the factory
import i18n from './i18n'; // Import i18n

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: AppServices;
    i18n: typeof i18n;
  }
}

export function ServicesPlugin() {
  return ({ store }: PiniaPluginContext) => {
    const isMockApi = import.meta.env.VITE_USE_MOCK === 'true';
    const mode: ServiceMode = isMockApi ? 'mock' : 'real';
    store.services = createServices(mode);
    store.i18n = i18n;
  };
}

// This is for global registration in main.ts
export default {
  install(app: App) {
    // No app-level installation needed for Pinia plugins, they are passed directly to Pinia
  },
};

