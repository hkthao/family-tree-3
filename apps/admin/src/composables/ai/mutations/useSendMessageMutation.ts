import { useMutation } from '@tanstack/vue-query';
import type { UseMutationOptions } from '@tanstack/vue-query';
import type { ApiError, AiChatMessage, Result, ChatResponse, ChatAttachmentDto } from '@/types';
import type { IChatService } from '@/services/chat/chat.service.interface';
import { useServices } from '@/plugins/services.plugin';

interface SendMessagePayload {
  familyId: string;
  sessionId: string;
  chatInput: string; // Changed from message to chatInput
  attachments?: ChatAttachmentDto[]; // New property for attachments
}

export function useSendMessageMutation(
  options?: UseMutationOptions<AiChatMessage, ApiError, SendMessagePayload>
) {
  const services = useServices();
  const chatService: IChatService = services.chat;

  return useMutation<AiChatMessage, ApiError, SendMessagePayload>(
    {
      mutationFn: async (payload: SendMessagePayload) => {
        const result: Result<ChatResponse, ApiError> = await chatService.sendMessage(payload.familyId, payload.sessionId, payload.chatInput, payload.attachments);
        if (result.ok) {
          return {
            sender: 'ai',
            text: result.value.output || '',
            intent: result.value.intent,
            generatedData: result.value.generatedData,
          } as AiChatMessage;
        } else {
          throw result.error;
        }
      },
      ...options,
    }
  );
}