import { setActivePinia, createPinia } from 'pinia';
import { useUserSettingsStore } from '@/stores/user-settings.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApiError, UserPreference } from '@/types';
import { Theme, Language } from '@/types';
import { ok, err } from '@/types';
import { createServices } from '@/services/service.factory';
import i18n from '@/plugins/i18n';

// Mock the ICurrentUserPreferenceService
const mockGetUserPreferences = vi.fn();
const mockSaveUserPreferences = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    user: {
      getUserPreferences: mockGetUserPreferences,
      saveUserPreferences: mockSaveUserPreferences,
    },
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      locale: { value: 'en' },
      t: vi.fn((key) => key),
    },
  },
}));

describe('user-settings.store', () => {
  let store: ReturnType<typeof useUserSettingsStore>;

  const mockUserPreference: UserPreference = {
    id: 'pref-1',
    userProfileId: 'user-1',
    theme: Theme.Dark,
    language: Language.English,
    created: '2023-01-01T00:00:00Z',
    createdBy: 'user-1',
    lastModified: null,
    lastModifiedBy: null,
  };

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useUserSettingsStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');
    // Reset mocks before each test
    mockGetUserPreferences.mockReset();
    mockSaveUserPreferences.mockReset();

    mockGetUserPreferences.mockResolvedValue(ok(mockUserPreference));
    mockSaveUserPreferences.mockResolvedValue(ok(true));
  });

  it('should have correct initial state', () => {
    expect(store.preferences.theme).toBe(Theme.Dark);
    expect(store.preferences.language).toBe(Language.English);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('fetchUserSettings', () => {
    it('should fetch settings successfully', async () => {
      await store.fetchUserSettings();

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.preferences).toEqual(mockUserPreference);
      expect(mockGetUserPreferences).toHaveBeenCalledTimes(1);
    });

    it('should handle fetch settings failure', async () => {
      const errorMessage = 'Failed to fetch user preferences.';
      mockGetUserPreferences.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.fetchUserSettings();

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(mockGetUserPreferences).toHaveBeenCalledTimes(1);
    });
  });

  describe('saveUserSettings', () => {
    it('should save settings successfully', async () => {
      const result = await store.saveUserSettings();

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockSaveUserPreferences).toHaveBeenCalledTimes(1);
      expect(mockSaveUserPreferences).toHaveBeenCalledWith(store.preferences);
    });

    it('should handle save settings failure', async () => {
      const errorMessage = 'Failed to save user preferences.';
      mockSaveUserPreferences.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.saveUserSettings();

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(mockSaveUserPreferences).toHaveBeenCalledTimes(1);
      expect(mockSaveUserPreferences).toHaveBeenCalledWith(store.preferences);
    });
  });

  it('should set the theme correctly', () => {
    store.setTheme(Theme.Light);
    expect(store.preferences.theme).toBe(Theme.Light);
    store.setTheme(Theme.Dark);
    expect(store.preferences.theme).toBe(Theme.Dark);
  });

  it('should set the language correctly and update i18n', () => {
    store.setLanguage(Language.Vietnamese);
    expect(store.preferences.language).toBe(Language.Vietnamese);
    expect(i18n.global.locale.value).toBe('vi');

    store.setLanguage(Language.English);
    expect(store.preferences.language).toBe(Language.English);
    expect(i18n.global.locale.value).toBe('en');
  });
});