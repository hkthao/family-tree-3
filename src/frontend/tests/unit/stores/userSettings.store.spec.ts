
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { useUserSettingsStore } from '@/stores/userSettings.store';
import i18n from '@/plugins/i18n';
import { createPinia, setActivePinia } from 'pinia';
import { Theme, Language } from '@/types';

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      locale: { value: 'en' },
      t: vi.fn((key) => key),
    },
  },
}));

describe('UserSettings Store', () => {
  let store: ReturnType<typeof useUserSettingsStore>;

  beforeEach(() => {
    setActivePinia(createPinia());
    store = useUserSettingsStore();
    store.$reset();
    vi.useFakeTimers(); // Use fake timers for setTimeout
  });

  afterEach(() => {
    vi.useRealTimers(); // Restore real timers
  });

  it('should have correct initial state', () => {
    expect(store.preferences.theme).toBe(Theme.Light);
    expect(store.preferences.emailNotificationsEnabled).toBe(true);
    expect(store.preferences.smsNotificationsEnabled).toBe(false);
    expect(store.preferences.inAppNotificationsEnabled).toBe(true);
    expect(store.preferences.language).toBe(Language.English);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  it('should set the theme correctly', () => {
    store.setTheme(Theme.Light);
    expect(store.preferences.theme).toBe(Theme.Light);
    store.setTheme(Theme.Dark);
    expect(store.preferences.theme).toBe(Theme.Dark);
  });

  it('should toggle notifications correctly', () => {
    expect(store.preferences.emailNotificationsEnabled).toBe(true);
    store.toggleEmailNotifications();
    expect(store.preferences.emailNotificationsEnabled).toBe(false);
    store.toggleEmailNotifications();
    expect(store.preferences.emailNotificationsEnabled).toBe(true);

    expect(store.preferences.smsNotificationsEnabled).toBe(false);
    store.toggleSmsNotifications();
    expect(store.preferences.smsNotificationsEnabled).toBe(true);

    expect(store.preferences.inAppNotificationsEnabled).toBe(true);
    store.toggleInAppNotifications();
    expect(store.preferences.inAppNotificationsEnabled).toBe(false);
  });

  it('should set the language correctly and update i18n', () => {
    store.setLanguage(Language.Vietnamese);
    expect(store.preferences.language).toBe(Language.Vietnamese);
    expect(i18n.global.locale.value).toBe('vi');

    store.setLanguage(Language.English);
    expect(store.preferences.language).toBe(Language.English);
    expect(i18n.global.locale.value).toBe('en');
  });

  describe('saveUserSettings', () => {
    it('should save settings successfully', async () => {
      // Mock Math.random to ensure success
      vi.spyOn(Math, 'random').mockReturnValue(0.5); // > 0.2 for success

      const promise = store.saveUserSettings();
      expect(store.loading).toBe(true);
      expect(store.error).toBeNull();

      vi.advanceTimersByTime(1000);
      const result = await promise;

      expect(store.loading).toBe(false);
      expect(result).toBe('userSettings.saveSuccess');
    });

    it('should handle save settings failure', async () => {
      // Mock Math.random to ensure failure
      vi.spyOn(Math, 'random').mockReturnValue(0.1); // < 0.2 for failure

      const promise = store.saveUserSettings();
      expect(store.loading).toBe(true);
      expect(store.error).toBeNull();

      vi.advanceTimersByTime(1000);

      await expect(promise).rejects.toThrow('userSettings.saveError');

      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.saveError');
    });
  });
});
