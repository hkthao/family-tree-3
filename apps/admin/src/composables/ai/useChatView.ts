import { ref, computed, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuth } from '@/composables';
import type { UserProfile, ChatLocation } from '@/types'; // Import ChatLocation
import { useAiChat } from '@/composables/ai/aiChat.composable'; // This will be the actual chat logic using services
import type { UploadedFile } from '@/composables/chat/useChatInput'; // Import UploadedFile

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
  const currentLocation = ref<ChatLocation | null>(null); // NEW: To store current location
  const { messages: messagesRef, loading, sendMessage: sendAiMessage } = useAiChat(familyId);
  const { state: authState } = useAuth();
  const userProfile = authState.currentUser as Ref<UserProfile | null>; // Fix: Remove redundant computed wrapper

  const suggestionChips = computed(() => [
    t('aiChat.suggestions.greeting'),
    t('aiChat.suggestions.whatIsFamilyTree'),
    t('aiChat.suggestions.howToAddMember'),
    t('aiChat.suggestions.tellMeAboutFamily'),
  ]);

  const sendMessage = async (attachments?: UploadedFile[]) => {
    if ((newMessage.value.trim() || (attachments && attachments.length > 0) || currentLocation.value) && !loading.value) {
      const messageText = newMessage.value;
      newMessage.value = '';
      await sendAiMessage(messageText, attachments, currentLocation.value || undefined); // Pass location, convert null to undefined
      currentLocation.value = null; // Clear location after sending
    }
  };

  const selectSuggestion = (suggestion: string) => {
    newMessage.value = suggestion;
  };

  const handleAddLocation = (location: ChatLocation) => { // Type the location
    currentLocation.value = location;
    // Optionally, send the message immediately after adding location
    // sendMessage();
  };

  return {
    state: {
      newMessage,
      messages: messagesRef.value,
      loading,
      userProfile,
      suggestionChips: suggestionChips.value,
    },
    actions: {
      sendMessage,
      selectSuggestion,
      handleAddLocation, // Expose handleAddLocation
    },
  };
}