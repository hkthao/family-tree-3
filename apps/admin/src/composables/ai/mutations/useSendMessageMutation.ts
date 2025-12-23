import { useMutation } from '@tanstack/vue-query';
import type { UseMutationOptions } from '@tanstack/vue-query';
import type { ApiError, AiChatMessage, Result, ChatResponse } from '@/types';
import type { IChatService } from '@/services/chat/chat.service.interface';
import { useServices } from '@/plugins/services.plugin';

interface SendMessagePayload {
  familyId: string;
  sessionId: string;
  message: string;
}

export function useSendMessageMutation(
  options?: UseMutationOptions<AiChatMessage, ApiError, SendMessagePayload>
) {
  const services = useServices();
  const chatService: IChatService = services.chat;

  return useMutation<AiChatMessage, ApiError, SendMessagePayload>(
    {
      mutationFn: async (payload: SendMessagePayload) => {
        const result: Result<ChatResponse, ApiError> = await chatService.sendMessage(payload.familyId, payload.sessionId, payload.message);
        if (result.ok) {
          return { sender: 'ai', text: result.value.output } as AiChatMessage;
        } else {
          throw result.error;
        }
      },
      ...options,
    }
  );
}