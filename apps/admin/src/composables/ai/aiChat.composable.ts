import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

interface Message {
  sender: 'user' | 'ai';
  text: string;
}

export function useAiChat(familyId: string) {
  const { t } = useI18n();
  const messages = ref<Message[]>([
    { sender: 'ai', text: t('aiChat.welcomeMessage') },
  ]);
  const loading = ref(false);

  const sendMessage = async (messageText: string) => {
    messages.value.push({ sender: 'user', text: messageText });
    loading.value = true;

    // Simulate API call
    try {
      // In a real application, you would make an API call here
      // For example:
      // const response = await apiClient.post(`/api/ai-chat/${familyId}`, { message: messageText });
      // messages.value.push({ sender: 'ai', text: response.data.reply });

      // Placeholder for AI response
      await new Promise(resolve => setTimeout(resolve, 1500)); // Simulate network delay
      messages.value.push({ sender: 'ai', text: `Chào bạn, tôi là Trợ lý AI của gia đình ${familyId}. Bạn muốn hỏi gì thêm?` });

    } catch (error) {
      console.error('AI chat error:', error);
      messages.value.push({ sender: 'ai', text: 'Xin lỗi, tôi gặp sự cố. Vui lòng thử lại sau.' });
    } finally {
      loading.value = false;
    }
  };

  return {
    messages,
    loading,
    sendMessage,
  };
}