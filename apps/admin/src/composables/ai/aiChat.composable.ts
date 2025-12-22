import { ref } from 'vue'; // Keep watch for local effects that are still needed here
import { useI18n } from 'vue-i18n';
import { v4 as uuidv4 } from 'uuid'; // Import v4 for uuid generation
import { useSendMessageMutation } from '@/composables/ai/mutations/useSendMessageMutation'; // NEW
import type { AiChatMessage } from '@/types'; // Import the new message type

// Define dependencies for useAiChat composable
interface UseAiChatDeps {
  useSendMessageMutation: typeof useSendMessageMutation;
}

const defaultAiChatDeps: UseAiChatDeps = {
  useSendMessageMutation: useSendMessageMutation,
};

export function useAiChat(familyId: string, deps: UseAiChatDeps = defaultAiChatDeps) {
  const { useSendMessageMutation } = deps;
  const { t } = useI18n();
  const sessionId = uuidv4(); // Generate a session ID once per chat session
  const messages = ref<AiChatMessage[]>([
    { sender: 'ai', text: t('aiChat.welcomeMessage') },
  ]);

  const {
    mutate: sendAiMessageMutation,
    isPending: isSendingMessage,
  } = useSendMessageMutation();

  const sendMessage = async (messageText: string) => {
    messages.value.push({ sender: 'user', text: messageText });

    sendAiMessageMutation(
      { familyId, sessionId, message: messageText },
      {
        onSuccess: (responseAiMessage) => {
          messages.value.push(responseAiMessage);
        },
        onError: (error) => {
          console.error('AI chat error:', error);
          messages.value.push({ sender: 'ai', text: 'Xin lỗi, tôi gặp sự cố. Vui lòng thử lại sau.' });
        },
      }
    );
  };

  return {
    messages,
    loading: isSendingMessage,
    sendMessage,
  };
}