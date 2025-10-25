import type { PiniaPluginContext } from 'pinia';
import {
  createServices,
  type ServiceMode,
  type AppServices,
} from '@/services/service.factory'; // Import the factory
import i18n from './i18n'; // Import i18n

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: AppServices;
    i18n: typeof i18n;
  }
}

export function ServicesPlugin() {
  return ({ store }: PiniaPluginContext) => {
    const mode: ServiceMode = 'real';
    store.services = createServices(mode);
    store.i18n = i18n;
  };
}

// This is for global registration in main.ts
export default {
  install() {
    // No app-level installation needed for Pinia plugins, they are passed directly to Pinia
  },
};
