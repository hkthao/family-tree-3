<template>
  <v-navigation-drawer v-model="chatOpen" location="right" temporary width="400" class="n8n-chat-window">
    <v-card flat class="fill-height d-flex flex-column">
      <v-sheet class="n8n-chat-container pa-0 d-flex flex-column" height="100%">
        <div class="mt-8 mb-1 pa-1 pb-0">
          <FamilyAutocomplete
            v-model="selectedFamilyId"
            label="Chọn gia đình để tra cứu"
            clearable
            hide-details
          />
        </div>
          <div id="n8n-chat-target" ></div>
          <v-btn class="btn-close" variant="text" density="compact" icon @click="toggleChat">
            <v-icon>mdi-close</v-icon>
          </v-btn>
      </v-sheet>
    </v-card>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, computed, nextTick, reactive } from 'vue';
import { createChat } from '@n8n/chat';
import { useI18n } from 'vue-i18n';
import '@n8n/chat/style.css';
import { useUserSettingsStore } from '@/stores/user-settings.store';
import { useAuthService } from '@/services/auth/authService';
import { Language } from '@/types';
import { getEnvVariable } from '@/utils/api.util';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue'; // Import FamilyAutocomplete

const { t } = useI18n();
const chatOpen = ref(false);
let chatInstance: ReturnType<typeof createChat> | null = null;
const userSettingsStore = useUserSettingsStore();
const authService = useAuthService();
const selectedFamilyId = ref<string | undefined>(undefined); // Declare familyId ref
const chatMetadata = reactive({
  familyId: selectedFamilyId.value,
});

// Watch for changes in selectedFamilyId and update the reactive metadata
watch(selectedFamilyId, (newId) => {
  chatMetadata.familyId = newId;
});

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

const initializeChat = async () => {
  if (chatInstance) return;
  const WEBHOOK_URL = getEnvVariable('VITE_N8N_CHAT_WEBHOOK_URL');
  if (!WEBHOOK_URL) {
    console.error('VITE_N8N_CHAT_WEBHOOK_URLK_URL is not defined. N8n chat widget will not function.');
    return;
  }

  const accessToken = await authService.getAccessToken();

  chatInstance = createChat({
    webhookUrl: WEBHOOK_URL,
    webhookConfig: {
      method: 'POST',
      headers: {
        'authorization': `Bearer ${accessToken}`,
      },
    },
    target: '#n8n-chat-target',
    mode: 'fullscreen',
    chatInputKey: 'chatInput',
    chatSessionKey: 'sessionId',
    loadPreviousSession: false,
    metadata: chatMetadata, // Pass the reactive metadata object
    showWelcomeScreen: false,
     // @ts-expect-error: The n8n chat widget expects 'vi' and 'en' for languages, not Language enum values.
    defaultLanguage: currentChatLanguage.value,
    initialMessages: [
     t('n8nChat.welcomeMessage'),
    ],
    i18n: {
      en: {
        title: 'Gia Phả Việt AI Assistant 👋',
        subtitle: "Your guide to family history. Select a family to get specific information.",
        footer: '',
        inputPlaceholder: 'Type your question..',
        getStarted: 'New Conversation',
        closeButtonTooltip: 'Close chat',
      },
      vi: {
        title: 'Trợ lý AI Gia Phả Việt 👋',
        subtitle: "Người bạn đồng hành trong hành trình tìm hiểu gia phả. Chọn một gia đình để tra cứu thông tin cụ thể.",
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

watch(() => userSettingsStore.preferences.language, (newLang, oldLang) => {
  if (newLang && newLang !== oldLang) {
    if (chatInstance) {
      chatInstance.unmount();
      chatInstance = null;
    }
    initializeChat();
  } else if (newLang && !chatInstance) {
    initializeChat();
  }
});

// The metadata property is now a reactive object, so it will dynamically get the latest selectedFamilyId.value
// No need to explicitly watch selectedFamilyId here to re-initialize chat.

onUnmounted(() => {
  if (chatInstance) {
    chatInstance.unmount();
    chatInstance = null;
  }
});
</script>

<style scoped>
.n8n-chat-window {
  display: flex;
  flex-direction: column;
  border-radius: 8px;
  overflow: hidden;
  position: relative;

}

#n8n-chat-target {
  height: calc(100vh - 140px) !important;
}

.btn-close {
  position: absolute;
  z-index: 10;
  top: 5px;
  left: 5px;
  color: rgb(var(--v-theme-primary));
}
</style>