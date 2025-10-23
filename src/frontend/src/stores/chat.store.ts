import { defineStore } from 'pinia';
import type { ChatListItem, MessageItem } from '@/types';
import type { ApiError } from '@/plugins/axios';

interface ChatState {
  chatList: ChatListItem[];
  selectedChatId: string | null;
  messages: { [chatId: string]: MessageItem[] };
  isLoading: boolean;
  error: string | null;
}

export const useChatStore = defineStore('chat', {
  state: (): ChatState => ({
    chatList: [],
    selectedChatId: null,
    messages: {},
    isLoading: false,
    error: null,
  }),

  getters: {
    currentChatMessages: (state) => {
      return state.selectedChatId
        ? state.messages[state.selectedChatId] || []
        : [];
    },
    currentChat: (state) => {
      return state.chatList.find((chat) => chat.id === state.selectedChatId);
    },
  },

  actions: {
    async fetchChatList(): Promise<Result<void, string>> {
      // In a real application, this would fetch from an API
      // For now, we use dummy data
      // this.chatList = await this.services.chat.fetchChats(); // Example usage
      if (this.chatList.length > 0 && !this.selectedChatId) {
        this.selectedChatId = this.chatList[0].id;
      }
      return { ok: true, value: undefined };
    },

    selectChat(chatId: string, t: (key: string) => string) {
      this.selectedChatId = chatId;
      if (!this.messages[chatId]) {
        this.messages[chatId] = [];
        // If it's the AI assistant chat and no messages exist, add an initial greeting
        if (chatId === 'ai-assistant' && this.messages[chatId].length === 0) {
          this.messages[chatId].push({
            id: Date.now().toString() + '_initial',
            senderId: 'ai-assistant',
            content: t('chat.initialMessage'),
            timestamp: new Date().toLocaleTimeString(),
            direction: 'incoming',
          });
        }
      }
    },

    async sendMessage(userMessage: string, currentUserId: string): Promise<Result<void, ApiError>> {
      if (!this.selectedChatId) {
        const errorMessage = 'No chat selected.';
        this.error = errorMessage;
        return { ok: false, error: { message: errorMessage } as ApiError };
      }

      this.isLoading = true;
      this.error = null;

      const newMessage: MessageItem = {
        id: Date.now().toString(),
        senderId: currentUserId,
        content: userMessage,
        timestamp: new Date().toLocaleTimeString(),
        direction: 'outgoing',
      };

      if (!this.messages[this.selectedChatId]) {
        this.messages[this.selectedChatId] = [];
      }
      this.messages[this.selectedChatId].push(newMessage);

      try {
        const result = await this.services.chat.sendMessage(
          userMessage,
          this.selectedChatId,
        );

        if (result.ok) {
          const botResponse = result.value;
          const botMessage: MessageItem = {
            id: Date.now().toString() + '_bot',
            senderId: this.selectedChatId, // Bot's ID is the chat ID
            content: botResponse.response,
            timestamp: new Date().toLocaleTimeString(),
            direction: 'incoming',
          };
          this.messages[this.selectedChatId].push(botMessage);

          // Update last message in chat list
          const chatIndex = this.chatList.findIndex(
            (chat) => chat.id === this.selectedChatId,
          );
          if (chatIndex !== -1) {
            this.chatList[chatIndex].lastMessage = botResponse.response;
            this.chatList[chatIndex].updatedAt = botResponse.createdAt
              ? new Date(botResponse.createdAt).toLocaleTimeString()
              : new Date().toLocaleTimeString();
          }
          return { ok: true, value: undefined };
        } else {
          this.error = result.error?.message || 'Failed to send message.';
          console.error('Error sending message:', result.error);
          const errorMessage: MessageItem = {
            id: Date.now().toString() + '_error',
            senderId: this.selectedChatId,
            content: 'Error: Could not get a response from the bot.',
            timestamp: new Date().toLocaleTimeString(),
            direction: 'incoming',
          };
          this.messages[this.selectedChatId].push(errorMessage);
          return { ok: false, error: result.error };
        }
      } catch (error: any) {
        this.error = error.message || 'Failed to send message.';
        console.error('Error sending message:', error);
        const errorMessage: MessageItem = {
          id: Date.now().toString() + '_error',
          senderId: this.selectedChatId,
          content: 'Error: Could not get a response from the bot.',
          timestamp: new Date().toLocaleTimeString(),
          direction: 'incoming',
        };
        this.messages[this.selectedChatId].push(errorMessage);
        return { ok: false, error: { message: this.error } as ApiError };
      } finally {
        this.isLoading = false;
      }
    },

    addMessage(chatId: string, message: MessageItem) {
      if (!this.messages[chatId]) {
        this.messages[chatId] = [];
      }
      this.messages[chatId].push(message);
    },

    updateLastMessage(chatId: string, lastMessage: string, updatedAt: string) {
      const chatIndex = this.chatList.findIndex((chat) => chat.id === chatId);
      if (chatIndex !== -1) {
        this.chatList[chatIndex].lastMessage = lastMessage;
        this.chatList[chatIndex].updatedAt = updatedAt;
      }
    },
  },
});
