import { setActivePinia, createPinia } from 'pinia';
import { useUserSettingsStore } from '@/stores/userSettings.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { UserPreference } from '@/types';
import { Theme, Language } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';
import i18n from '@/plugins/i18n';

// Mock the IUserPreferenceService
const mockGetUserPreferences = vi.fn();
const mockSaveUserPreferences = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    userPreference: {
      getUserPreferences: mockGetUserPreferences,
      saveUserPreferences: mockSaveUserPreferences,
    },
    // Add other services as empty objects if they are not directly used by userSettings.store
    ai: {},
    auth: {},
    chat: {},
    chunk: {},
    dashboard: {},
    event: {},
    face: {},
    faceMember: {},
    family: {},
    fileUpload: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userProfile: {},
    userSettings: {},
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

describe('userSettings.store', () => {
  let store: ReturnType<typeof useUserSettingsStore>;

  const mockUserPreference: UserPreference = {
    id: 'pref-1',
    userProfileId: 'user-1',
    theme: Theme.Dark,
    language: Language.English,
    emailNotificationsEnabled: true,
    smsNotificationsEnabled: false,
    inAppNotificationsEnabled: true,
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
    store.services = createServices('mock');
    // Reset mocks before each test
    mockGetUserPreferences.mockReset();
    mockSaveUserPreferences.mockReset();

    mockGetUserPreferences.mockResolvedValue(ok(mockUserPreference));
    mockSaveUserPreferences.mockResolvedValue(ok(true));
  });

  it('should have correct initial state', () => {
    expect(store.preferences.theme).toBe(Theme.Dark);
    expect(store.preferences.language).toBe(Language.English);
    expect(store.preferences.emailNotificationsEnabled).toBe(true);
    expect(store.preferences.smsNotificationsEnabled).toBe(false);
    expect(store.preferences.inAppNotificationsEnabled).toBe(true);
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

  it('should toggle email notifications correctly', () => {
    store.preferences.emailNotificationsEnabled = true; // Ensure initial state for toggle
    store.toggleEmailNotifications();
    expect(store.preferences.emailNotificationsEnabled).toBe(false);
    store.toggleEmailNotifications();
    expect(store.preferences.emailNotificationsEnabled).toBe(true);
  });

  it('should toggle sms notifications correctly', () => {
    store.preferences.smsNotificationsEnabled = false; // Ensure initial state for toggle
    store.toggleSmsNotifications();
    expect(store.preferences.smsNotificationsEnabled).toBe(true);
    store.toggleSmsNotifications();
    expect(store.preferences.smsNotificationsEnabled).toBe(false);
  });

  it('should toggle in-app notifications correctly', () => {
    store.preferences.inAppNotificationsEnabled = true; // Ensure initial state for toggle
    store.toggleInAppNotifications();
    expect(store.preferences.inAppNotificationsEnabled).toBe(false);
    store.toggleInAppNotifications();
    expect(store.preferences.inAppNotificationsEnabled).toBe(true);
  });
});