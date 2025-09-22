import type { App } from 'vue';
import type { PiniaPluginContext } from 'pinia';
import { MockFamilyService, ApiFamilyService, MockMemberService, ApiMemberService } from '@/services';
import type { IFamilyService, IMemberService } from '@/services'; // Use import type for interfaces

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: {
      family: IFamilyService;
      member: IMemberService; // Added member service
      // Add other services here as they are created
    };
  }
}

export function ServicesPlugin({ store }: PiniaPluginContext) {
  const isMockApi = import.meta.env.VITE_APP_USE_MOCK_API === 'true';

  const familyService: IFamilyService = isMockApi
    ? new MockFamilyService()
    : new ApiFamilyService();

  const memberService: IMemberService = isMockApi // Added member service instantiation
    ? new MockMemberService()
    : new ApiMemberService();

  // Inject services into the store
  Object.defineProperty(store, 'services', {
    value: {
      family: familyService,
      member: memberService, // Added member service to value
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
