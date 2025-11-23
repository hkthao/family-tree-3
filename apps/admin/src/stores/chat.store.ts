import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { IChatService } from '@/services/chat/chat.service.interface';
import { v4 as uuidv4 } from 'uuid'; // Import uuid

// No longer need to import useServiceContainer directly here

// Define message structure for vue-advanced-chat
interface ChatMessage {
  _id: string;
  content: string;
  senderId: string; // 'user' or 'assistant'
  username: string; // Display name of the sender
  timestamp: string; // Unix timestamp as string
  date: string; // Formatted date string
  isError?: boolean; // Optional flag for error messages
}

interface ChatListItem {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  updatedAt: string;
}

export const useChatStore = defineStore('chat', {
  state: () => ({
    // State for the currently active chat
    selectedChatId: ref<string | null>(null),
    messages: ref<Record<string, ChatMessage[]>>({}), // Messages grouped by chat ID
    isLoading: ref(false),
    error: ref<string | null>(null),
    aiAssistantSessionId: ref<string>(uuidv4()), // New: Session ID for AI assistant chat
    minTypingDelay: ref(500), // Minimum time the typing indicator is shown (in ms)

    // State for the chat list (if multiple chats are supported)
    chatList: ref<ChatListItem[]>([]),
  }),

  getters: {
    // Computed property for messages of the currently selected chat
    currentChatMessages(state): ChatMessage[] {
      return state.selectedChatId ? state.messages[state.selectedChatId] || [] : [];
    },
  },

  actions: {
    // Action to select a chat
    selectChat(chatId: string, i18nTranslator: any) {
      this.selectedChatId = chatId;
      if (!this.messages[chatId]) {
        this.messages[chatId] = [];
        // Add initial message for AI assistant if it's a new chat
        if (chatId === 'ai-assistant' && this.messages[chatId].length === 0) {
          this.messages[chatId].push({
            _id: 'initial-ai-message',
            content: i18nTranslator('chat.initialMessage'),
            senderId: 'assistant',
            username: 'AI Assistant',
            timestamp: new Date().toTimeString().substring(0, 5),
            date: new Date().toLocaleDateString(),
          });
        }
      }
    },

    // Action to send a message
    async sendMessage(sessionId: string, messageContent: string, currentUserId: string, i18nTranslator: any) {
      this.error = null;

      if (!this.selectedChatId) {
        this.error = i18nTranslator('chat.errors.noChatSelected');
        return;
      }

      const userMessage: ChatMessage = {
        _id: Date.now().toString(),
        content: messageContent,
        senderId: currentUserId,
        timestamp: new Date().toTimeString().substring(0, 5),
        date: new Date().toLocaleDateString(),
        username: ''
      };
      this.addMessage(this.selectedChatId, userMessage);

      const tempMessageId = `temp_${Date.now()}`;
      const tempMessage: ChatMessage = {
        _id: tempMessageId,
        content: '...💬', // Chat bubble emoji for typing indicator
        senderId: 'assistant',
        username: 'AI Assistant',
        timestamp: new Date().toTimeString().substring(0, 5),
        date: new Date().toLocaleDateString(),
      };
      this.addMessage(this.selectedChatId, tempMessage);
      try {
        const chatService = this.services.chat as IChatService;
        
        const result = await chatService.sendMessage(sessionId, messageContent);

        // Remove the temporary message (typing indicator)
        const tempMessageIndex = this.messages[this.selectedChatId].findIndex(m => m._id === tempMessageId);
        if (tempMessageIndex !== -1) {
          this.messages[this.selectedChatId].splice(tempMessageIndex, 1);
        }

        if (result.ok) {
          const finalMessage: ChatMessage = {
            _id: (Date.now() + 1).toString(),
            content: result.value || i18nTranslator('chat.errors.emptyResponse'),
            senderId: 'assistant',
            username: 'AI Assistant',
            timestamp: new Date().toTimeString().substring(0, 5),
            date: new Date().toLocaleDateString(),
          };
          this.addMessage(this.selectedChatId, finalMessage);
        } else {
          throw result.error; // Throw the ApiError to be caught by the catch block
        }

      } catch (err: any) {
        console.error('Error sending message:', err);
        const errorMessage = err.message || 'An unknown error occurred.';

        // If error, replace the typing indicator with an error message
        const tempMessageIndex = this.messages[this.selectedChatId].findIndex(m => m._id === tempMessageId);
        if (tempMessageIndex !== -1) {
          this.messages[this.selectedChatId][tempMessageIndex].content = errorMessage;
          this.messages[this.selectedChatId][tempMessageIndex].isError = true;
        } else {
          const errorMsg: ChatMessage = {
            _id: (Date.now() + 1).toString(),
            content: errorMessage,
            senderId: 'assistant',
            username: 'AI Assistant',
            timestamp: new Date().toTimeString().substring(0, 5),
            date: new Date().toLocaleDateString(),
            isError: true,
          };
          this.addMessage(this.selectedChatId, errorMsg);
        }
      } finally {
        // Ensure isLoading is true for at least minTypingDelay
        await Promise.all([
          new Promise(resolve => setTimeout(resolve, this.minTypingDelay)),
          Promise.resolve(), // To ensure Promise.all always has at least two promises
        ]);
        this.isLoading = false;
      }
    },

    // Helper to add a message to a specific chat
    addMessage(chatId: string, message: ChatMessage) {
      if (!this.messages[chatId]) {
        this.messages[chatId] = [];
      }
      this.messages[chatId].push(message);
      // Update the last message in chatList
      this.updateLastMessage(chatId, message.content, message.timestamp, message.date);
    },

    // Helper to update the last message in chatList
    updateLastMessage(chatId: string, content: string, timestamp: string, date: string) {
      const chatItem = this.chatList.find(chat => chat.id === chatId);
      if (chatItem) {
        chatItem.lastMessage = content;
        // Combine date and timestamp to create a proper Date object
        const fullDateTimeString = `${date} ${timestamp}`;
        chatItem.updatedAt = new Date(fullDateTimeString).toLocaleTimeString();
      }
    },

    // Action to fetch chat list (placeholder for now)
    async fetchChatList(i18nTranslator: any) {
      // In a real app, this would fetch from a backend
      // For now, we'll just ensure the AI assistant chat exists
      if (!this.chatList.some(chat => chat.id === 'ai-assistant')) {
        this.chatList.push({
          id: 'ai-assistant',
          name: 'AI Assistant',
          avatar: '', // Placeholder
          lastMessage: i18nTranslator('chat.initialMessage'),
          updatedAt: new Date().toLocaleTimeString(),
        });
      }
    },
  },
});