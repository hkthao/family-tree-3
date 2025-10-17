import { setActivePinia, createPinia } from 'pinia';
import { useNotificationStore } from '@/stores/notification.store';
import { beforeEach, describe, expect, it } from 'vitest';

describe('notification.store', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  it('should show snackbar with default color', () => {
    const store = useNotificationStore();
    store.showSnackbar('Test Message');
    expect(store.snackbar.show).toBe(true);
    expect(store.snackbar.message).toBe('Test Message');
    expect(store.snackbar.color).toBe('success');
  });

  it('should show snackbar with custom color', () => {
    const store = useNotificationStore();
    store.showSnackbar('Error Message', 'error');
    expect(store.snackbar.show).toBe(true);
    expect(store.snackbar.message).toBe('Error Message');
    expect(store.snackbar.color).toBe('error');
  });

  it('should hide snackbar', () => {
    const store = useNotificationStore();
    // First, show the snackbar to ensure it's visible
    store.showSnackbar('Test Message');
    expect(store.snackbar.show).toBe(true);

    // Then, hide it
    store.hideSnackbar();
    expect(store.snackbar.show).toBe(false);
  });
});
