import type { PiniaPluginContext } from 'pinia';
import {
  createServices,
  type ServiceMode,
  type AppServices,
} from '@/services/service.factory'; // Import the factory
import i18n from './i18n'; // Import i18n
import { inject, provide } from 'vue'; // NEW: Import inject and provide

// NEW: Define a symbol for the injection key
export const ServicesInjectionKey = Symbol('AppServices');

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: AppServices;
    i18n: typeof i18n;
  }
}

export function ServicesPlugin() {
  return ({ store }: PiniaPluginContext) => {
    const mode: ServiceMode = 'real';
    const services = createServices(mode); // Create services once
    store.services = services;
    store.i18n = i18n;
  };
}

// NEW: Composable to use services in Vue components
export function useServices(): AppServices {
  const services = inject<AppServices>(ServicesInjectionKey);
  if (!services) {
    throw new Error('AppServices not provided. Ensure ServicesPlugin is installed correctly.');
  }
  return services;
}

// This is for global registration in main.ts
export default {
  install(app: any) { // Changed app to any to avoid type issues with Vue app
    const mode: ServiceMode = 'real';
    const services = createServices(mode);
    app.provide(ServicesInjectionKey, services); // Provide the services globally
  },
};
