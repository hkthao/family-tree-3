<template>
  <v-navigation-drawer v-model="chatOpen" location="right" temporary width="400" class="chat-window">

    <v-card flat class="fill-height d-flex flex-column">
      <v-card-text class="chat-messages-container pa-0">
        <vue-advanced-chat :messages="formattedMessages" :current-user-id="currentUserId"
          :is-loading="chatStore.isLoading" :show-footer="true" :messages-loaded="messagesLoaded"
          :rooms="JSON.stringify([{ roomId: 'ai-assistant', roomName: 'AI Assistant', users: [{ _id: currentUserId, username: authStore.user?.id || 'You' }, { _id: 'assistant', username: 'AI Assistant' }] }])"
          :rooms-loaded="true" :single-room="true" :typing-users="typingUsers" :show-audio="false" :auto-scroll="JSON.stringify({
            send: {
              new: true,
              newAfterScrollUp: false
            },
            receive: {
              new: false,
              newAfterScrollUp: true
            }
          })" @send-message="handleSendMessage">
        </vue-advanced-chat>

        <v-btn class="btn-close" variant="text" density="compact" icon @click="toggleChat">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-card-text>
      <v-alert v-if="chatStore.error" type="error" dense dismissible class="ma-2">
        {{ chatStore.error }}
      </v-alert>
    </v-card>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useChatStore, useAuthStore } from '@/stores';
import { useI18n } from 'vue-i18n';
import { register } from 'vue-advanced-chat'
register()

const { t } = useI18n();
const chatOpen = ref(false);
const chatStore = useChatStore();
const authStore = useAuthStore();
const messagesLoaded = ref(false);

const currentUserId = computed(() => authStore.user?.id || 'guest');

// Prop to control visibility from parent (TopBar)
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['update:modelValue']);

// Watch for changes in modelValue to update chatOpen
watch(() => props.modelValue, (newVal) => {
  chatOpen.value = newVal;
  if (newVal && !chatStore.selectedChatId) {
    // Automatically select AI assistant chat when opened
    chatStore.selectChat('ai-assistant', t);
  }
});

const formattedMessages = computed(() => {
  const messages = chatStore.currentChatMessages.map(msg => ({
    _id: msg._id,
    content: String(msg.content),
    senderId: msg.senderId,
    username: msg.senderId === currentUserId.value ? authStore.user?.id || 'You' : 'AI Assistant',
    timestamp: msg.timestamp,
    date: msg.date,
    isError: msg.isError, // Pass the isError flag
  }));
  return messages;
});

const typingUsers = computed(() => {
  if (chatStore.isLoading) {
    return [{ _id: 'assistant', username: 'AI Assistant' }];
  }
  return [];
});

const toggleChat = () => {
  chatOpen.value = !chatOpen.value;
  emit('update:modelValue', chatOpen.value); // Emit update to parent
}

const handleSendMessage = async (message: CustomEvent<{ content: string }[]>) => {
  const messageContent = message.detail[0].content;
  if (messageContent && messageContent.trim()) {
    await chatStore.sendMessage(chatStore.aiAssistantSessionId, messageContent, currentUserId.value, t);
  }
};

onMounted(() => {
  chatStore.fetchChatList(t);
  messagesLoaded.value = true;
});
</script>

<style scoped>
.chat-window {
  width: 380px;
  height: fit-content !important;
  display: flex;
  flex-direction: column;
  border-radius: 8px;
  overflow: hidden;
  position: relative;
}

.btn-close {
  position: absolute;
  z-index: 10;
  top: 15px;
  right: 15px;
  color: rgb(var(--v-theme-primary));
}
</style>
