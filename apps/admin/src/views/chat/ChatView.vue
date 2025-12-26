<template>
  <v-card class="mx-auto d-flex flex-column chat-card" height="100%" flat :loading="state.loading.value">
    <v-card-text class="flex-grow-1 pa-0 ">
      <div class="chat-messages" ref="chatMessagesContainer">
        <div v-for="(message, index) in state.messages" :key="index"
          :class="['d-flex align-center my-1', message.sender === 'user' ? 'justify-end' : 'justify-start']">
          <template v-if="message.sender === 'user'">
            <UserChatMessage :message="message" :userProfile="state.userProfile" />
          </template>
          <template v-else>
            <AiChatMessage
              :message="message"
              :familyId="props.familyId"
              @open-relationship-detection="(familyId: string) => emit('open-relationship-detection', familyId)"
            />
          </template>
          <template v-if="!message.text">
            <!-- Debugging: log message if text is empty or not a string -->
            {{ console.log('Message without text property:', message) }}
          </template>
        </div>
        <div v-if="state.loading.value" class="d-flex justify-start">
          <v-chip class="ma-1" color="grey-lighten-1" label>
            <v-progress-circular indeterminate size="20" width="2"></v-progress-circular>
            <span class="ml-2">{{ t('aiChat.typing') }}</span>
          </v-chip>
        </div>
        <v-btn v-if="showScrollToBottomButton" icon variant="flat" size="small" color="primary"
          class="scroll-to-bottom-button" @click="scrollToBottom">
          <v-icon>mdi-arrow-down-circle</v-icon>
        </v-btn>
      </div>
    </v-card-text>
    <v-card-actions class="d-flex justify-center pa-0 chat-input-area">
      <ChatInput
        :model-value="state.newMessage.value"
        @update:model-value="handleUpdateNewMessage"
        :placeholder="t('aiChat.placeholder')"
        @sendMessage="handleSendMessage"
        :disabled="state.loading.value"
        :loading="state.loading.value"
        @addLocation="handleAddLocation"
      />
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, nextTick, onUnmounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useChatView } from '@/composables/ai/useChatView';
import UserChatMessage from '@/components/chat/UserChatMessage.vue';
import AiChatMessage from '@/components/chat/AiChatMessage.vue';
import ChatInput from '@/components/chat/ChatInput.vue';
import type { UploadedFile } from '@/composables/chat/useChatInput'; // Import UploadedFile

const props = defineProps<{
  familyId: string;
}>();

const emit = defineEmits([
  'open-relationship-detection',
]);

const { t } = useI18n(); // Keep t for template
const chatMessagesContainer = ref<HTMLElement | null>(null);
const { state, actions } = useChatView(props.familyId);
const showScrollToBottomButton = ref(false); // New ref for button visibility

const handleUpdateNewMessage = (newValue: string) => {
  state.newMessage.value = newValue;
};

const handleSendMessage = (attachments?: UploadedFile[]) => {
  actions.sendMessage(attachments);
};

const handleAddLocation = (location: { latitude: number; longitude: number; address?: string }) => {
  actions.handleAddLocation(location);
};

const SCROLL_THRESHOLD = 100; // Pixels from the bottom to consider "near bottom"

const scrollToBottom = () => {
  if (chatMessagesContainer.value) {
    chatMessagesContainer.value.scrollTop = chatMessagesContainer.value.scrollHeight;
  }
};

const handleScroll = () => {
  if (chatMessagesContainer.value) {
    const { scrollTop, scrollHeight, clientHeight } = chatMessagesContainer.value;
    // Show button if not at the bottom (with a small tolerance)
    showScrollToBottomButton.value = scrollHeight - scrollTop > clientHeight + SCROLL_THRESHOLD;
  }
};

// Watch for new messages
watch(state.messages, (newMessages, oldMessages) => {
  if (chatMessagesContainer.value) {
    const { scrollTop, scrollHeight, clientHeight } = chatMessagesContainer.value;
    const isNearBottom = scrollHeight - scrollTop <= clientHeight + SCROLL_THRESHOLD;

    // If a new message arrived and the user was near the bottom, auto-scroll
    if (newMessages.length > oldMessages.length && isNearBottom) {
      nextTick(() => {
        scrollToBottom();
        showScrollToBottomButton.value = false; // Hide button as we are at the bottom
      });
    } else if (newMessages.length > oldMessages.length && !isNearBottom) {
      // If new message arrived and user is scrolled up, show the button
      showScrollToBottomButton.value = true;
    }
  }
}, { deep: true });

// Add event listener for scroll when component is mounted
nextTick(() => {
  if (chatMessagesContainer.value) {
    chatMessagesContainer.value.addEventListener('scroll', handleScroll);
    // Initial check in case chat is pre-filled and not at bottom
    handleScroll();
  }
});

// Cleanup event listener when component is unmounted
onUnmounted(() => {
  if (chatMessagesContainer.value) {
    chatMessagesContainer.value.removeEventListener('scroll', handleScroll);
  }
});
</script>

<style scoped>
.chat-card {
  position: relative;
}

.chat-messages {
  overflow-y: auto;
  max-height: calc(100vh - 230px);
  /* Adjusted for larger input area */
}

.chat-input-area {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: var(--v-card-background);
}

.scroll-to-bottom-button {
  position: absolute;
  bottom: 135px;
  /* Adjust as needed to be above the input field */
  right: 32px;
  z-index: 10;
  /* Ensure it's above other content */
}

.message-content {
  white-space: pre-wrap;
  /* Preserves whitespace and wraps text */
  word-break: break-word;
  /* Ensures long words break to prevent overflow */
}
</style>
