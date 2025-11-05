import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { IChatService } from '@/services/chat/chat.service.interface';
// No longer need to import useServiceContainer directly here

// Define message structure for vue-advanced-chat
interface ChatMessage {
  _id: string;
  content: string;
  senderId: string; // 'user' or 'assistant'
  username: string; // Display name of the sender
  timestamp: string; // Unix timestamp as string
  date: string; // Formatted date string
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
    async sendMessage(messageContent: string, currentUserId: string) {
      const { t } = useI18n(); // Get i18n instance within action

      this.isLoading = true;
      this.error = null;

      if (!this.selectedChatId) {
        this.error = t('chat.errors.noChatSelected');
        this.isLoading = false;
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

      let assistantMessage: ChatMessage | undefined; // Declare outside try block

      try {
        assistantMessage = { // Assign here
          _id: (Date.now() + 1).toString(),
          content: '',
          senderId: 'assistant',
          username: 'AI Assistant',
          timestamp: new Date().toTimeString().substring(0, 5),
          date: new Date().toLocaleDateString(),
        };
        this.addMessage(this.selectedChatId, assistantMessage);

        // Access chatService via this.services
        const chatService = this.services.chat as IChatService;
        const responseStream = chatService.sendMessageStream(messageContent);
        for await (const chunk of responseStream) {
          assistantMessage.content += chunk;
          // Update the last message in chatList for the current chat
          this.updateLastMessage(this.selectedChatId, assistantMessage.content, assistantMessage.timestamp);
        }
      } catch (err: any) {
        console.error('Error sending message:', err);
        this.error = t('chat.errors.sendMessageFailed', { message: err.message });
        // Remove the assistant's empty message if an error occurred before any content was streamed
        if (this.currentChatMessages[this.currentChatMessages.length - 1] === assistantMessage && assistantMessage.content === '') {
          this.messages[this.selectedChatId].pop();
        }
      } finally {
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
      this.updateLastMessage(chatId, message.content, message.timestamp);
    },

    // Helper to update the last message in chatList
    updateLastMessage(chatId: string, content: string, timestamp: string) {
      const chatItem = this.chatList.find(chat => chat.id === chatId);
      if (chatItem) {
        chatItem.lastMessage = content;
        chatItem.updatedAt = new Date(parseInt(timestamp)).toLocaleTimeString();
      }
    },

    // Action to fetch chat list (placeholder for now)
    async fetchChatList() {
      const { t } = useI18n(); // Get i18n instance within action
      // In a real app, this would fetch from a backend
      // For now, we'll just ensure the AI assistant chat exists
      if (!this.chatList.some(chat => chat.id === 'ai-assistant')) {
        this.chatList.push({
          id: 'ai-assistant',
          name: 'AI Assistant',
          avatar: '', // Placeholder
          lastMessage: t('chat.initialMessage'),
          updatedAt: new Date().toLocaleTimeString(),
        });
      }
    },
  },
});