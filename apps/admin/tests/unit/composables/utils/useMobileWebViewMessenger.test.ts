import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { useMobileWebViewMessenger } from '@/composables/utils/useMobileWebViewMessenger';

describe('useMobileWebViewMessenger', () => {
  let originalReactNativeWebView: any;

  beforeEach(() => {
    originalReactNativeWebView = (window as any).ReactNativeWebView;
    // Reset window.ReactNativeWebView for each test
    delete (window as any).ReactNativeWebView;
  });

  afterEach(() => {
    // Restore original window.ReactNativeWebView
    (window as any).ReactNativeWebView = originalReactNativeWebView;
  });

  it('isReactNativeWebView returns true when ReactNativeWebView is present', () => {
    (window as any).ReactNativeWebView = { postMessage: vi.fn() };
    const { isReactNativeWebView } = useMobileWebViewMessenger();
    expect(isReactNativeWebView()).toBe(true);
  });

  it('isReactNativeWebView returns false when ReactNativeWebView is not present', () => {
    const { isReactNativeWebView } = useMobileWebViewMessenger();
    expect(isReactNativeWebView()).toBe(false);
  });

  it('postMapSelectionMessage calls postMessage when ReactNativeWebView is present', () => {
    const mockPostMessage = vi.fn();
    (window as any).ReactNativeWebView = { postMessage: mockPostMessage };

    const { postMapSelectionMessage } = useMobileWebViewMessenger();
    const data = {
      coordinates: { latitude: 10, longitude: 20 },
      location: 'Test Location',
    };
    postMapSelectionMessage(data);

    expect(mockPostMessage).toHaveBeenCalledTimes(1);
    expect(mockPostMessage).toHaveBeenCalledWith(JSON.stringify(data));
  });

  it('postMapSelectionMessage does not call postMessage when ReactNativeWebView is not present', () => {
    // Ensure ReactNativeWebView is not present
    delete (window as any).ReactNativeWebView;

    const { postMapSelectionMessage } = useMobileWebViewMessenger();
    const data = {
      coordinates: { latitude: 10, longitude: 20 },
      location: 'Test Location',
    };
    // If postMessage were called, it would throw an error or be a mock that we didn't set up.
    // We expect it to do nothing.
    expect(() => postMapSelectionMessage(data)).not.toThrow();
  });
});