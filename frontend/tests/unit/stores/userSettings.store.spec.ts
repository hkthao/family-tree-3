
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { useUserSettingsStore } from '@/stores/userSettings.store';
import i18n from '@/plugins/i18n';
import { createPinia, setActivePinia } from 'pinia';

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
    expect(store.theme).toBe('dark');
    expect(store.notifications).toEqual({
      email: true,
      sms: false,
      inApp: true,
    });
    expect(store.language).toBe('en');
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  it('should set the theme correctly', () => {
    store.setTheme('light');
    expect(store.theme).toBe('light');
    store.setTheme('dark');
    expect(store.theme).toBe('dark');
  });

  it('should toggle notifications correctly', () => {
    expect(store.notifications.email).toBe(true);
    store.toggleNotification('email');
    expect(store.notifications.email).toBe(false);
    store.toggleNotification('email');
    expect(store.notifications.email).toBe(true);

    expect(store.notifications.sms).toBe(false);
    store.toggleNotification('sms');
    expect(store.notifications.sms).toBe(true);

    expect(store.notifications.inApp).toBe(true);
    store.toggleNotification('inApp');
    expect(store.notifications.inApp).toBe(false);
  });

  it('should set the language correctly and update i18n', () => {
    store.setLanguage('vi');
    expect(store.language).toBe('vi');
    expect(i18n.global.locale.value).toBe('vi');

    store.setLanguage('en');
    expect(store.language).toBe('en');
    expect(i18n.global.locale.value).toBe('en');
  });

  describe('saveSettings', () => {
    it('should save settings successfully', async () => {
      // Mock Math.random to ensure success
      vi.spyOn(Math, 'random').mockReturnValue(0.5); // > 0.2 for success

      const promise = store.saveSettings();
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

      const promise = store.saveSettings();
      expect(store.loading).toBe(true);
      expect(store.error).toBeNull();

      vi.advanceTimersByTime(1000);

      await expect(promise).rejects.toThrow('userSettings.saveError');

      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.saveError');
    });
  });
});
