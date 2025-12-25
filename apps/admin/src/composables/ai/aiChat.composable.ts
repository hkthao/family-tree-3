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
    {
      sender: 'ai', text: t('aiChat.welcomeMessage'),
    },
    // {
    //   sender: 'ai', text: `Dưới đây là một số thông tin chính về ứng dụng Gia Phả Việt:
    // - Ứng dụng Gia Phả Việt là hệ thống quản lý gia phả toàn diện, giúp người dùng tạo, quản lý, trực quan hóa và chia sẻ cây gia phả một cách dễ dàng.
    // - Ứng dụng tập trung vào bảo tồn lịch sử gia đình, cung cấp các công cụ trực quan để theo dõi các thành viên và mối quan hệ trong gia đình.
    // - Người dùng có thể thêm thông tin, sự kiện liên quan đến thành viên gia đình, cũng như tạo các tiểu sử, câu chuyện giúp lưu giữ ký ức gia đình.
    // - Ứng dụng hỗ trợ tùy chọn sử dụng AI để tạo nội dung tiểu sử hay hỗ trợ quản lý dữ liệu gia đình.
    // - Người dùng cần chịu trách nhiệm cung cấp thông tin chính xác và sử dụng các tính năng một cách có trách nhiệm, tôn trọng quyền riêng tư và các quy định hiện hành.

    // Nếu bạn cần hướng dẫn chi tiết hơn về các tính năng hoặc cách sử dụng ứng dụng, tôi sẵn sàng hỗ trợ thêm nhé!`,
    //   generatedData: {
    //     events: [
    //       {
    //         "id": "00000000-0000-0000-0000-000000000000",
    //         "name": "Sinh nhật Huỳnh Gia Bảo",
    //         "code": "event_birthday_hgb",
    //         "description": "Sự kiện sinh nhật của Huỳnh Gia Bảo",
    //         "calendarType": 1,
    //         "solarDate": null,
    //         "lunarDate": null,
    //         "repeatRule": 1,
    //         "familyId": null,
    //         "type": 0,
    //         "relatedMembers": [],
    //         "relatedMemberIds": []
    //       }
    //     ],
    //     families: [],
    //     members: [
    //       {
    //         "id": "00000000-0000-0000-0000-000000000000",
    //         "firstName": "Nhật Nam",
    //         "lastName": "Huỳnh",
    //         "code": "HN1990",
    //         "isDeceased": false,
    //         "familyId": "00000000-0000-0000-0000-000000000000",
    //         "isRoot": false
    //       },
    //       {
    //         "id": "00000000-0000-0000-0000-000000000000",
    //         "firstName": "Hải",
    //         "lastName": "Phạm",
    //         "code": "PH2005",
    //         "isDeceased": false,
    //         "familyId": "00000000-0000-0000-0000-000000000000",
    //         "isRoot": false
    //       }
    //     ] as MemberDto[],
    //   },
    //   intent: "RELATIONSHIP_LOOKUP_PAGE"
    // },
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