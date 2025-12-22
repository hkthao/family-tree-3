import { ref, computed, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuth } from '@/composables';
import type { UserProfile } from '@/types'; // Import AiChatMessage
import { useAiChat } from '@/composables/ai/aiChat.composable'; // This will be the actual chat logic using services

interface UseChatViewDeps {
  useI18n: typeof useI18n;
  useAuth: typeof useAuth;
  useAiChat: (familyId: string) => ReturnType<typeof useAiChat>;
}

const defaultDeps: UseChatViewDeps = {
  useI18n: useI18n,
  useAuth: useAuth,
  useAiChat: useAiChat,
};

export function useChatView(familyId: string, deps: UseChatViewDeps = defaultDeps) {
  const { useI18n, useAuth, useAiChat } = deps;
  const { t } = useI18n();

  const newMessage = ref('');
  const { messages: messagesRef, loading, sendMessage: sendAiMessage } = useAiChat(familyId);
  const { state: authState } = useAuth();
  const userProfile = authState.currentUser as Ref<UserProfile | null>; // Fix: Remove redundant computed wrapper

  const suggestionChips = computed(() => [
    t('aiChat.suggestions.greeting'),
    t('aiChat.suggestions.whatIsFamilyTree'),
    t('aiChat.suggestions.howToAddMember'),
    t('aiChat.suggestions.tellMeAboutFamily'),
  ]);

  const sendMessage = async () => {
    if (newMessage.value.trim() && !loading.value) {
      const messageText = newMessage.value;
      newMessage.value = '';
      await sendAiMessage(messageText);
    }
  };

  const selectSuggestion = (suggestion: string) => {
    newMessage.value = suggestion;
  };

  return {
    state: {
      newMessage,
      messages: messagesRef.value, // No longer need explicit cast here
      loading,
      userProfile,
      suggestionChips: suggestionChips.value, // Pass the value of the computed ref
    },
    actions: {
      sendMessage,
      selectSuggestion,
    },
  };
}