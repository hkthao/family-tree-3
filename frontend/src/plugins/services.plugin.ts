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

interface ServicesPluginOptions {
  mockFamilyService?: IFamilyService;
  mockMemberService?: IMemberService;
}

export function ServicesPlugin(options: ServicesPluginOptions = {}) {
  return ({ store }: PiniaPluginContext) => {
    const isMockApi = import.meta.env.VITE_APP_USE_MOCK_API === 'true';

    const familyService: IFamilyService = options.mockFamilyService
      ? options.mockFamilyService
      : (isMockApi ? new MockFamilyService() : new ApiFamilyService());

    const memberService: IMemberService = options.mockMemberService
      ? options.mockMemberService
      : (isMockApi ? new MockMemberService() : new ApiMemberService());

    // Inject services into the store
    Object.defineProperty(store, 'services', {
      value: {
        family: familyService,
        member: memberService,
        // Add other services here
      },
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
