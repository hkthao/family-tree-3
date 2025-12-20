// tests/unit/composables/chat/n8nChat.adapter.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ApiN8nChatAdapter } from '@/composables/chat/n8nChat.adapter';
import { createChat } from '@n8n/chat';

// Mock the @n8n/chat library
vi.mock('@n8n/chat', () => ({
  createChat: vi.fn(),
}));

// Mock document.querySelector for DOM manipulation
const mockQuerySelector = vi.fn();
const mockUnmount = vi.fn();

describe('ApiN8nChatAdapter', () => {
  let adapter: ApiN8nChatAdapter;

  beforeEach(() => {
    vi.clearAllMocks();
    adapter = new ApiN8nChatAdapter();
    // Mock createChat to return a chat instance with an unmount method
    vi.mocked(createChat).mockReturnValue({ unmount: mockUnmount } as any);

    // Mock document.querySelector to return a dummy element
    mockQuerySelector.mockReturnValue(document.createElement('div'));
    vi.spyOn(document, 'querySelector').mockImplementation(mockQuerySelector);
  });

  it('should initialize createChat and mount when mount is called', () => {
    const options = {
      webhookUrl: 'http://localhost:5678',
      accessToken: 'test_token',
      targetSelector: '#n8n-chat-target',
      metadata: { familyId: '123' },
      defaultLanguage: 'en',
      i18n: { en: { title: 'English Title' } },
    };

    adapter.mount(options);

    expect(document.querySelector).toHaveBeenCalledWith(options.targetSelector);
    expect(createChat).toHaveBeenCalledWith(
      expect.objectContaining({
        webhookUrl: options.webhookUrl,
        webhookConfig: {
          method: 'POST',
          headers: {
            'authorization': `Bearer ${options.accessToken}`,
          },
        },
        target: options.targetSelector,
        mode: 'fullscreen',
        metadata: options.metadata,
        defaultLanguage: options.defaultLanguage,
        i18n: options.i18n,
      }),
    );
    expect(adapter.isMounted()).toBe(true);
  });

  it('should not initialize if chat instance is already mounted', () => {
    const options = {
      webhookUrl: 'http://localhost:5678',
      accessToken: 'test_token',
      targetSelector: '#n8n-chat-target',
      metadata: { familyId: '123' },
      defaultLanguage: 'en',
      i18n: { en: { title: 'English Title' } },
    };

    adapter.mount(options); // First mount
    adapter.mount(options); // Second mount

    expect(createChat).toHaveBeenCalledTimes(1); // Should only be called once
  });

  it('should not initialize if targetSelector is not found', () => {
    mockQuerySelector.mockReturnValue(null); // Mock querySelector to return null

    const options = {
      webhookUrl: 'http://localhost:5678',
      accessToken: 'test_token',
      targetSelector: '#n8n-chat-target',
      metadata: { familyId: '123' },
      defaultLanguage: 'en',
      i18n: { en: { title: 'English Title' } },
    };

    adapter.mount(options);

    expect(document.querySelector).toHaveBeenCalledWith(options.targetSelector);
    expect(createChat).not.toHaveBeenCalled();
    expect(adapter.isMounted()).toBe(false);
  });

  it('should call unmount on the chat instance when unmount is called', () => {
    const options = {
      webhookUrl: 'http://localhost:5678',
      accessToken: 'test_token',
      targetSelector: '#n8n-chat-target',
      metadata: { familyId: '123' },
      defaultLanguage: 'en',
      i18n: { en: { title: 'English Title' } },
    };
    adapter.mount(options);
    expect(adapter.isMounted()).toBe(true);

    adapter.unmount();
    expect(mockUnmount).toHaveBeenCalledTimes(1);
    expect(adapter.isMounted()).toBe(false);
  });

  it('should not throw error if unmount is called when not mounted', () => {
    expect(() => adapter.unmount()).not.toThrow();
  });
});
