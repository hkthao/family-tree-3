<template>
  <v-navigation-drawer v-model="chatOpen" location="right" temporary width="400" class="n8n-chat-window">
    <v-card flat class="fill-height d-flex flex-column">
      <v-card-text class="n8n-chat-container pa-0">
        <div id="n8n-chat-target" class="fill-height"></div>
        <v-btn class="btn-close" variant="text" density="compact" icon @click="toggleChat">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-card-text>
    </v-card>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { createChat } from '@n8n/chat';
import { useI18n } from 'vue-i18n';
import '@n8n/chat/style.css';

const { t } = useI18n();
const chatOpen = ref(false);

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['update:modelValue']);

watch(() => props.modelValue, (newVal) => {
  chatOpen.value = newVal;
});

const toggleChat = () => {
  chatOpen.value = !chatOpen.value;
  emit('update:modelValue', chatOpen.value);
};

onMounted(() => {
  const WEBHOOK_URL = import.meta.env.VITE_N8N_WEBHOOK_URL || 'https://n8n.yourdomain.com/webhook-test/chat';

  createChat({
    webhookUrl: WEBHOOK_URL,
    webhookConfig: {
      method: 'POST',
      headers: {},
    },
    target: '#n8n-chat-target',
    mode: 'fullscreen',
    chatInputKey: 'chatInput',
    chatSessionKey: 'sessionId',
    loadPreviousSession: true,
    metadata: {},
    showWelcomeScreen: false,
    defaultLanguage: 'en',
    initialMessages: [
      t('n8nChat.initialMessageHi'),
      t('n8nChat.initialMessageNathan'),
    ],
    i18n: {
      en: {
        title: 'Hi there! 👋',
        subtitle: "Start a chat. We're here to help you 24/7.",
        footer: '',
        inputPlaceholder: 'Type your question..',
        getStarted: 'New Conversation',
        closeButtonTooltip: 'Close chat',
      },
      vi: {
        title: 'Chào bạn! 👋',
        subtitle: "Bắt đầu cuộc trò chuyện. Chúng tôi luôn sẵn sàng hỗ trợ bạn 24/7.",
        footer: '',
        inputPlaceholder: 'Nhập câu hỏi của bạn..',
        getStarted: 'Cuộc trò chuyện mới',
        closeButtonTooltip: 'Đóng trò chuyện',
      },
    },
    enableStreaming: false,
  });
});
</script>

<style scoped>
.n8n-chat-window {
  width: 380px;
  height: fit-content !important;
  display: flex;
  flex-direction: column;
  border-radius: 8px;
  overflow: hidden;
  position: relative;
}

.n8n-chat-container {
  height: 100%;
  position: relative;
}

.btn-close {
  position: absolute;
  z-index: 10;
  top: 15px;
  right: 15px;
  color: rgb(var(--v-theme-primary));
}

/* Ensure the n8n chat takes full height within its container */
#n8n-chat-target >>> .n8n-chat-root {
  height: 100%;
}
</style>