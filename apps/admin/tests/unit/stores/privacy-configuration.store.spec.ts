import { setActivePinia, createPinia } from 'pinia';
import { usePrivacyConfigurationStore, type PrivacyConfiguration } from '@/stores/privacy-configuration.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IPrivacyConfigurationService
const mockGetPrivacyConfiguration = vi.fn();
const mockUpdatePrivacyConfiguration = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    family: {
      getPrivacyConfiguration: mockGetPrivacyConfiguration,
      updatePrivacyConfiguration: mockUpdatePrivacyConfiguration,
    },
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key), // Mock the translation function to return the key itself
    },
  },
}));

describe('privacy-configuration.store', () => {
  let store: ReturnType<typeof usePrivacyConfigurationStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = usePrivacyConfigurationStore();
    store.$reset();
    store.services = createServices('test');
    mockGetPrivacyConfiguration.mockReset();
    mockUpdatePrivacyConfiguration.mockReset();

    // Default mock resolved values
    mockGetPrivacyConfiguration.mockResolvedValue(ok(mockPrivacyConfig));
    mockUpdatePrivacyConfiguration.mockResolvedValue(ok(undefined));
  });

  const mockPrivacyConfig: PrivacyConfiguration = {
    id: 'config-1',
    familyId: 'family-1',
    publicMemberProperties: ['fullName', 'dateOfBirth'],
  };

  describe('fetchPrivacyConfiguration', () => {
    it('should fetch privacy configuration successfully', async () => {
      mockGetPrivacyConfiguration.mockResolvedValue(ok(mockPrivacyConfig));

      const result = await store.fetchPrivacyConfiguration('family-1');

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.privacyConfig).toEqual(mockPrivacyConfig);
      expect(result.ok).toBe(true);
      if (result.ok) {
        expect(result.value).toEqual(mockPrivacyConfig);
      }
      expect(mockGetPrivacyConfiguration).toHaveBeenCalledTimes(1);
      expect(mockGetPrivacyConfiguration).toHaveBeenCalledWith('family-1');
    });

    it('should handle fetch failure', async () => {
      const errorMessage = 'Failed to fetch config.';
      mockGetPrivacyConfiguration.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.fetchPrivacyConfiguration('family-1');

      expect(store.loading).toBe(false);
      expect(store.error).toBe('privacyConfiguration.errors.fetch');
      expect(store.privacyConfig).toBeNull();
      expect(result.ok).toBe(false);
      if (!result.ok) {
        expect(result.error?.message).toBe(errorMessage);
      }
      expect(mockGetPrivacyConfiguration).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockGetPrivacyConfiguration.mockResolvedValue(ok(mockPrivacyConfig));
      const promise = store.fetchPrivacyConfiguration('family-1');

      expect(store.loading).toBe(true);
      await promise;
      expect(store.loading).toBe(false);
    });
  });

  describe('updatePrivacyConfiguration', () => {
    it('should update privacy configuration successfully', async () => {
      mockUpdatePrivacyConfiguration.mockResolvedValue(ok(undefined));
      store.privacyConfig = { ...mockPrivacyConfig, familyId: 'family-2' }; // Set initial state with different familyId

      const result = await store.updatePrivacyConfiguration('family-1', ['fullName']);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockUpdatePrivacyConfiguration).toHaveBeenCalledTimes(1);
      expect(mockUpdatePrivacyConfiguration).toHaveBeenCalledWith('family-1', ['fullName']);
      // Should not update privacyConfig in store if familyId does not match initially
      expect(store.privacyConfig?.publicMemberProperties).not.toEqual(['fullName']);
    });

    it('should update privacyConfig in store if familyId matches', async () => {
      mockUpdatePrivacyConfiguration.mockResolvedValue(ok(undefined));
      store.privacyConfig = { ...mockPrivacyConfig }; // Set initial state

      const result = await store.updatePrivacyConfiguration('family-1', ['fullName']);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.privacyConfig?.publicMemberProperties).toEqual(['fullName']); // Should update
    });


    it('should handle update failure', async () => {
      const errorMessage = 'Failed to update config.';
      mockUpdatePrivacyConfiguration.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.updatePrivacyConfiguration('family-1', ['fullName']);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('privacyConfiguration.errors.update');
      expect(mockUpdatePrivacyConfiguration).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockUpdatePrivacyConfiguration.mockResolvedValue(ok(undefined));
      const promise = store.updatePrivacyConfiguration('family-1', ['fullName']);

      expect(store.loading).toBe(true);
      await promise;
      expect(store.loading).toBe(false);
    });
  });

  describe('clearPrivacyConfiguration', () => {
    it('should clear the privacyConfig', () => {
      store.privacyConfig = mockPrivacyConfig;
      expect(store.privacyConfig).toEqual(mockPrivacyConfig);

      store.clearPrivacyConfiguration();

      expect(store.privacyConfig).toBeNull();
    });
  });
});