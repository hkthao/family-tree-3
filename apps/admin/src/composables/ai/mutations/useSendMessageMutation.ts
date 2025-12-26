import { useMutation } from '@tanstack/vue-query';
import type { UseMutationOptions } from '@tanstack/vue-query';
import type { ApiError, AiChatMessage, Result, ChatResponse, ChatAttachmentDto, ChatLocation } from '@/types';
import type { IChatService } from '@/services/chat/chat.service.interface';
import { useServices } from '@/plugins/services.plugin';

interface SendMessagePayload {
  familyId: string;
  sessionId: string;
  chatInput: string;
  attachments?: ChatAttachmentDto[];
  location?: ChatLocation; // NEW: Add location property
}

export function useSendMessageMutation(
  options?: UseMutationOptions<AiChatMessage, ApiError, SendMessagePayload>
) {
  const services = useServices();
  const chatService: IChatService = services.chat;

  return useMutation<AiChatMessage, ApiError, SendMessagePayload>(
    {
      mutationFn: async (payload: SendMessagePayload) => {
        const result: Result<ChatResponse, ApiError> = await chatService.sendMessage(payload.familyId, payload.sessionId, payload.chatInput, payload.attachments, payload.location); // Pass location
        if (result.ok) {
          return {
            sender: 'ai',
            text: result.value.output || '',
            intent: result.value.intent,
            generatedData: result.value.generatedData,
            faceDetectionResults: result.value.faceDetectionResults
          } as AiChatMessage;
        } else {
          throw result.error;
        }
      },
      ...options,
    }
  );
}