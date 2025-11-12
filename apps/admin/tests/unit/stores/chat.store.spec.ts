import { setActivePinia, createPinia } from 'pinia';
import { useChatStore } from '@/stores/chat.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ChatResponse } from '@/types';
import { createServices } from '@/services/service.factory';
import { ok } from '@/types/result.d';

// Mock the IChatService
const mockSendMessage = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    chat: {
      sendMessage: mockSendMessage,
    },
    // Add other services as empty objects if they are not directly used by chat.store
    ai: {},
    auth: {},
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
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

describe('chat.store', () => {
  let store: ReturnType<typeof useChatStore>;

  // const mockChatListItem: ChatListItem = {
  //   id: 'chat-1',
  //   name: 'Test Chat',
  //   avatar: 'test-avatar',
  //   lastMessage: 'Hello',
  //   updatedAt: 'now',
  // };

  const mockChatResponse: ChatResponse = {
    response: 'Bot response',
    context: [],
    createdAt: new Date().toISOString(),
  };

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useChatStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');

    // Reset mocks before each test
    mockSendMessage.mockReset();

    // Set default mock resolved values
    mockSendMessage.mockResolvedValue(ok(mockChatResponse));
  });

  it('should have correct initial state', () => {
    expect(store.chatList).toEqual([]);
    expect(store.selectedChatId).toBeNull();
    expect(store.messages).toEqual({});
    expect(store.isLoading).toBe(false);
    expect(store.error).toBeNull();
  });
});