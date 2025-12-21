import { watch, onUnmounted, computed, reactive, type Ref, type ComputedRef } from 'vue';
import { useI18n } from 'vue-i18n';
interface ChatMetadata {
  familyId: string | undefined;
}
import { getEnvVariable } from '@/utils/api.util';

import { useAccessToken } from '../auth/useAccessToken';
import { useUserPreferences } from '../user/useUserPreferences';
import { type N8nChatAdapter, ApiN8nChatAdapter } from './n8nChat.adapter';
import { getN8nChatI18n } from './n8nChat.logic';

interface UseN8nChatDeps {
  n8nChatAdapter: N8nChatAdapter;
}

const defaultDeps = {
  n8nChatAdapter: new ApiN8nChatAdapter(),
};

export function useN8nChat(
  selectedFamilyId: Ref<string | undefined> | ComputedRef<string | undefined>,
  chatOpen: Ref<boolean>,
  deps: UseN8nChatDeps = defaultDeps
) {
  const { t } = useI18n();
  const { accessToken } = useAccessToken();
  const { state: { preferences, currentChatLanguage } } = useUserPreferences();
  const { n8nChatAdapter } = deps;

  const chatMetadata = reactive<ChatMetadata>({
    familyId: computed(() => selectedFamilyId.value).value, // Reactive familyId
  });

  // Watch for changes in selectedFamilyId and update the reactive metadata
  const handleFamilyIdChange = (newId: string | undefined) => {
    chatMetadata.familyId = newId;
  };
  watch(selectedFamilyId, handleFamilyIdChange);

  const WEBHOOK_URL = getEnvVariable('VITE_N8N_CHAT_WEBHOOK_URL');
  if (!WEBHOOK_URL) {
    console.error('VITE_N8N_CHAT_WEBHOOK_URL is not defined. N8n chat widget will not function.');
  }

  const initializeChat = async () => {
    // Don't initialize if already initialized, no webhook URL, no access token, or no preferences
    if (n8nChatAdapter.isMounted() || !WEBHOOK_URL || !accessToken.value || !preferences.value) {
      return;
    }

    // Pass targetSelector directly to the adapter
    n8nChatAdapter.mount({
      webhookUrl: WEBHOOK_URL,
      accessToken: accessToken.value,
      targetSelector: '#n8n-chat-target',
      metadata: chatMetadata,
      defaultLanguage: currentChatLanguage.value,
      i18n: getN8nChatI18n(t),
    });
  };

  const destroyChat = () => {
    n8nChatAdapter.unmount();
  };

  const handleChatDependenciesChange = async () => {
    const newAccessToken = accessToken.value;
    const newPreferences = preferences.value;
    const newChatOpen = chatOpen.value;


    const adapterIsMounted = n8nChatAdapter.isMounted();

    // Check if chat should be initialized or re-initialized
    if (newChatOpen && newAccessToken && newPreferences && WEBHOOK_URL) {
      // If chat is already mounted but configuration has changed (e.g., familyId, token, or language),
      // we need to destroy and remount to apply changes.
      // This is a simplification; a more advanced adapter might support dynamic updates.
      if (adapterIsMounted) {
        // Check if metadata (familyId) or access token or language changed
        // For simplicity, we just destroy and remount if any watched dependency changes.
        // The watch array includes selectedFamilyId, accessToken, preferences (which includes language)
        destroyChat();
      }
      await initializeChat();
    } else if (!newChatOpen && adapterIsMounted) {
      destroyChat();
    }
  };
  // Watch for access token, preferences, chatOpen, AND selectedFamilyId to initialize/destroy chat
  watch([accessToken, preferences, chatOpen, selectedFamilyId], handleChatDependenciesChange, { immediate: true });

  onUnmounted(() => {
    destroyChat();
  });

  return {
    state: {
      isChatReady: computed(() => n8nChatAdapter.isMounted()),
    },
    actions: {
      toggleChat: () => {
        chatOpen.value = !chatOpen.value;
      },
    },
  };
}
