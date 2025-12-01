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
import { ref, watch, onMounted, onUnmounted, computed, nextTick } from 'vue';
import { createChat } from '@n8n/chat';
import { useI18n } from 'vue-i18n';
import '@n8n/chat/style.css';
import { useUserSettingsStore } from '@/stores/user-settings.store';
import { useAuthService } from '@/services/auth/authService';
import { useServices } from '@/plugins/services.plugin'; // Updated import to use the composable
import { Language } from '@/types'; // Import Language enum

const { t } = useI18n();
const chatOpen = ref(false);
let chatInstance: ReturnType<typeof createChat> | null = null;
const userSettingsStore = useUserSettingsStore();
const authService = useAuthService(); // NEW
const services = useServices(); // NEW

// Initial fetch of user settings if not already loaded (optional, but good practice)
if (!userSettingsStore.preferences.language) {
  userSettingsStore.fetchUserSettings();
}

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

const currentChatLanguage = computed(() => {
  return userSettingsStore.preferences.language === Language.English ? 'en' : 'vi';
});

const initializeChat = async () => { // Changed to async
  if (chatInstance) return; // Prevent re-initialization if already created
  const WEBHOOK_URL = import.meta.env.VITE_N8N_CHAT_WEBHOOK_URL;
  if (!WEBHOOK_URL) {
    console.error('VITE_N8N_WEBHOOK_URL is not defined. N8n chat widget will not function.');
    return;
  }

  // NEW: Get JWT token
  const user = await authService.getUser();
  if (!user || !user.id) {
    console.error('User not logged in or user ID not available. Cannot get JWT for n8n chat.');
    return;
  }

  const jwtResult = await services.n8n.getWebhookJwt(user.id);

  if (!jwtResult.ok) {
    console.error('Failed to get n8n webhook JWT:', jwtResult.error?.message);
    return;
  }

  const jwtToken = jwtResult.value;

  chatInstance = createChat({
    webhookUrl: WEBHOOK_URL,
    webhookConfig: {
      method: 'POST',
      headers: {
        'authorization': `Bearer ${jwtToken}`, // Use dynamic token
      },
    },
    target: '#n8n-chat-target',
    mode: 'fullscreen',
    chatInputKey: 'chatInput',
    chatSessionKey: 'sessionId',
    loadPreviousSession: false,
    metadata: {},
    showWelcomeScreen: false,
    // @ts-expect-error: The n8n chat widget expects 'vi' and 'en' for languages, not Language enum values.
    defaultLanguage: currentChatLanguage.value,
    initialMessages: [
      t('n8nChat.welcomeMessage'),
    ],
    i18n: {
      en: {
        title: 'Gia Phả Việt AI Assistant 👋',
        subtitle: "Your guide to family history. Ask me anything!",
        footer: '',
        inputPlaceholder: 'Type your question..',
        getStarted: 'New Conversation',
        closeButtonTooltip: 'Close chat',
      },
      vi: {
        title: 'Trợ lý AI Gia Phả Việt 👋',
        subtitle: "Người bạn đồng hành trong hành trình tìm hiểu gia phả. Hãy hỏi tôi bất cứ điều gì!",
        footer: '',
        inputPlaceholder: 'Nhập câu hỏi của bạn..',
        getStarted: 'Cuộc trò chuyện mới',
        closeButtonTooltip: 'Đóng trò chuyện',
      },
    },
    enableStreaming: false,
  });
};

onMounted(() => {
  if (!userSettingsStore.preferences.language) {
    userSettingsStore.fetchUserSettings().then(() => {
      // After fetching, if language is available, initialize chat
      if (userSettingsStore.preferences.language) {
        initializeChat();
      }
    });
  } else {
    nextTick(() => {
      initializeChat();
    });
  }
});

// Watch for changes in user's language preference and re-initialize if necessary
watch(() => userSettingsStore.preferences.language, (newLang, oldLang) => {
  if (newLang && newLang !== oldLang && chatInstance) {
    // If language changes and chat is already initialized, unmount and re-initialize
    chatInstance.unmount();
    chatInstance = null; // Reset chatInstance
    initializeChat();
  } else if (newLang && !chatInstance) {
    // If language becomes available and chat is not initialized
    initializeChat();
  }
});

onUnmounted(() => {
  if (chatInstance) {
    chatInstance.unmount();
    chatInstance = null;
  }
});
</script>

<style scoped>
.n8n-chat-window {
  height: fit-content !important;
  display: flex;
  flex-direction: column;
  border-radius: 8px;
  overflow: hidden;
  position: relative;
}

.n8n-chat-container {
  height: 65vh;
}

.btn-close {
  position: absolute;
  z-index: 10;
  top: 15px;
  right: 15px;
  color: rgb(var(--v-theme-primary));
}
</style>