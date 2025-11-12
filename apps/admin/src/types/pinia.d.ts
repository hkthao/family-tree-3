import 'pinia';
import type { AppServices } from '@/services/service.factory';

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: AppServices;
  }
}