import { watch, onUnmounted, computed, nextTick, reactive, type Ref, type ComputedRef } from 'vue';
import { createChat } from '@n8n/chat';
import { useI18n } from 'vue-i18n';
import '@n8n/chat/style.css';
import { getEnvVariable } from '@/utils/api.util';

import { useAccessToken } from '../auth/useAccessToken';
import { useUserPreferences } from '../user/useUserPreferences';

export function useN8nChat(selectedFamilyId: Ref<string | undefined> | ComputedRef<string | undefined>, chatOpen: Ref<boolean>) {
  const { t } = useI18n();
  let chatInstance: ReturnType<typeof createChat> | null = null;

  const { accessToken } = useAccessToken();
  const { preferences, currentChatLanguage } = useUserPreferences();

  const chatMetadata = reactive({
    familyId: computed(() => selectedFamilyId.value).value, // Reactive familyId
  });

  // Watch for changes in selectedFamilyId and update the reactive metadata
  watch(selectedFamilyId, (newId) => {
    chatMetadata.familyId = newId;
  });

  const WEBHOOK_URL = getEnvVariable('VITE_N8N_CHAT_WEBHOOK_URL');
  if (!WEBHOOK_URL) {
    console.error('VITE_N8N_CHAT_WEBHOOK_URL is not defined. N8n chat widget will not function.');
  }

  const initializeChat = async () => {
    if (chatInstance || !WEBHOOK_URL || !accessToken.value || !preferences.value) {
      // Don't initialize if already initialized, no webhook URL, no access token, or no preferences
      return;
    }

    // Ensure the DOM target is available before creating the chat
    await nextTick();
    const chatTarget = document.querySelector('#n8n-chat-target');
    if (!chatTarget) {
      console.warn('#n8n-chat-target not found in DOM, deferring chat initialization.');
      return;
    }

    chatInstance = createChat({
      webhookUrl: WEBHOOK_URL,
      webhookConfig: {
        method: 'POST',
        headers: {
          'authorization': `Bearer ${accessToken.value}`,
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

  const destroyChat = () => {
    if (chatInstance) {
      chatInstance.unmount();
      chatInstance = null;
    }
  };

  // Watch for access token, preferences, and chatOpen state to initialize/destroy chat
  watch([accessToken, preferences, chatOpen], async ([newAccessToken, newPreferences, newChatOpen]) => {
    if (newChatOpen && newAccessToken && newPreferences) {
      await initializeChat();
    } else if (!newChatOpen && chatInstance) {
      destroyChat();
    }
  }, { immediate: true });

  onUnmounted(() => {
    destroyChat();
  });

  return {
    toggleChat: () => {
      chatOpen.value = !chatOpen.value;
    },
    isChatReady: computed(() => !!chatInstance),
  };
}
