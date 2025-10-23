import { setActivePinia, createPinia } from 'pinia';
import { useChatStore } from '@/stores/chat.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ChatListItem, MessageItem, ChatResponse } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

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
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

describe('chat.store', () => {
  let store: ReturnType<typeof useChatStore>;

  const mockChatListItem: ChatListItem = {
    id: 'chat-1',
    name: 'Test Chat',
    avatar: 'test-avatar',
    lastMessage: 'Hello',
    updatedAt: 'now',
  };

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
    // @ts-ignore
    store.services = createServices('mock');

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

  describe('getters', () => {
    it('currentChatMessages should return messages for selected chat', () => {
      store.selectedChatId = 'chat-1';
      store.messages['chat-1'] = [{ id: 'msg-1', senderId: 'user', content: 'Hi', timestamp: 'now', direction: 'outgoing' }];
      expect(store.currentChatMessages).toEqual([{ id: 'msg-1', senderId: 'user', content: 'Hi', timestamp: 'now', direction: 'outgoing' }]);
    });

    it('currentChatMessages should return empty array if no chat selected', () => {
      store.selectedChatId = null;
      expect(store.currentChatMessages).toEqual([]);
    });

    it('currentChat should return selected chat', () => {
      store.chatList = [mockChatListItem];
      store.selectedChatId = 'chat-1';
      expect(store.currentChat).toEqual(mockChatListItem);
    });

    it('currentChat should return undefined if no chat selected', () => {
      store.selectedChatId = null;
      expect(store.currentChat).toBeUndefined();
    });
  });

  describe('fetchChatList', () => {
    it('should fetch chat list successfully and select first chat if available', async () => {
      store.chatList = [mockChatListItem]; // Simulate pre-existing chat list
      const result = await store.fetchChatList();

      expect(result.ok).toBe(true);
      expect(store.selectedChatId).toBe('chat-1');
    });

    it('should not select chat if chat list is empty', async () => {
      store.chatList = [];
      const result = await store.fetchChatList();

      expect(result.ok).toBe(true);
      expect(store.selectedChatId).toBeNull();
    });
  });

  describe('selectChat', () => {
    it('should select a chat and initialize messages if not present', () => {
      store.selectChat('new-chat', vi.fn());
      expect(store.selectedChatId).toBe('new-chat');
      expect(store.messages['new-chat']).toEqual([]);
    });

    it('should add initial greeting for AI assistant chat if no messages exist', () => {
      const tMock = vi.fn((key) => key);
      store.selectChat('ai-assistant', tMock);
      expect(store.selectedChatId).toBe('ai-assistant');
      expect(store.messages['ai-assistant'].length).toBe(1);
      expect(store.messages['ai-assistant'][0].senderId).toBe('ai-assistant');
      expect(store.messages['ai-assistant'][0].content).toBe('chat.initialMessage');
      expect(tMock).toHaveBeenCalledWith('chat.initialMessage');
    });

    it('should not add initial greeting if messages already exist', () => {
      store.messages['ai-assistant'] = [{ id: 'existing', senderId: 'user', content: 'Hi', timestamp: 'now', direction: 'outgoing' }];
      store.selectChat('ai-assistant', vi.fn());
      expect(store.messages['ai-assistant'].length).toBe(1);
    });
  });

  describe('sendMessage', () => {
    const currentUserId = 'user-id';
    const userMessage = 'Hello bot';

    beforeEach(() => {
      store.selectedChatId = 'ai-assistant';
      store.messages['ai-assistant'] = [];
    });

    it('should send message successfully and receive bot response', async () => {
      const result = await store.sendMessage(userMessage, currentUserId);

      expect(result.ok).toBe(true);
      expect(store.isLoading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockSendMessage).toHaveBeenCalledTimes(1);
      expect(mockSendMessage).toHaveBeenCalledWith(userMessage, 'ai-assistant');
      expect(store.messages['ai-assistant'].length).toBe(2); // User message + bot response
      expect(store.messages['ai-assistant'][0].content).toBe(userMessage);
      expect(store.messages['ai-assistant'][1].content).toBe(mockChatResponse.response);
    });

    it('should handle send message failure', async () => {
      const errorMessage = 'Failed to send message.';
      mockSendMessage.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.sendMessage(userMessage, currentUserId);

      expect(result.ok).toBe(false);
      expect(store.isLoading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(mockSendMessage).toHaveBeenCalledTimes(1);
      expect(store.messages['ai-assistant'].length).toBe(2); // User message + error message
      expect(store.messages['ai-assistant'][1].content).toContain('Error:');
    });

    it('should return error if no chat selected', async () => {
      store.selectedChatId = null;
      const result = await store.sendMessage(userMessage, currentUserId);

      expect(result.ok).toBe(false);
      expect(store.isLoading).toBe(false);
      expect(store.error).toBe('No chat selected.');
      expect(mockSendMessage).not.toHaveBeenCalled();
    });
  });

  describe('addMessage', () => {
    it('should add a message to the specified chat', () => {
      const message: MessageItem = { id: 'new-msg', senderId: 'user', content: 'Test', timestamp: 'now', direction: 'outgoing' };
      store.addMessage('chat-2', message);
      expect(store.messages['chat-2']).toEqual([message]);
    });

    it('should add a message to an existing chat', () => {
      store.messages['chat-1'] = [{ id: 'existing', senderId: 'user', content: 'Hi', timestamp: 'now', direction: 'outgoing' }];
      const message: MessageItem = { id: 'new-msg', senderId: 'user', content: 'Test', timestamp: 'now', direction: 'outgoing' };
      store.addMessage('chat-1', message);
      expect(store.messages['chat-1'].length).toBe(2);
      expect(store.messages['chat-1'][1]).toEqual(message);
    });
  });

  describe('updateLastMessage', () => {
    it('should update the last message and updatedAt for a chat', () => {
      store.chatList = [mockChatListItem];
      store.updateLastMessage('chat-1', 'Updated message', 'updated-time');
      expect(store.chatList[0].lastMessage).toBe('Updated message');
      expect(store.chatList[0].updatedAt).toBe('updated-time');
    });

    it('should not update if chat not found', () => {
      store.chatList = [mockChatListItem];
      store.updateLastMessage('non-existent-chat', 'Updated message', 'updated-time');
      expect(store.chatList[0].lastMessage).toBe(mockChatListItem.lastMessage);
    });
  });
});