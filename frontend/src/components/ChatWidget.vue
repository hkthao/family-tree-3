<template>
  <div class="chat-widget-container">
    <v-fab-transition>
      <Vue3Lottie class="chat-bot-btn" @click="toggleChat" v-if="!chatOpen" :animationData="ChatbotAnimation"
        :height="150" :width="150" />
    </v-fab-transition>

    <v-card v-if="chatOpen" class="chat-window elevation-12">
      <v-toolbar color="primary" dark dense>
        <v-toolbar-title>{{ t('chat.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn icon @click="toggleChat">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-toolbar>
      <v-card-text class="chat-messages-container">
        <v-list class="chat-messages-list">
          <v-list-item density="compact" v-for="message in chatStore.currentChatMessages" :key="message.id"
            class="message-item" :class="{
              'message-outgoing': message.direction === 'outgoing',
              'message-incoming': message.direction === 'incoming',
            }">
            <template v-slot:prepend v-if="message.direction === 'incoming'">
              <v-avatar :size="32">
                <v-icon color="primary">mdi-robot</v-icon>
              </v-avatar>
            </template>
            <v-card density="compact" :color="message.direction === 'outgoing' ? 'primary' : 'grey lighten-2'"
              :dark="message.direction === 'outgoing'" class="message-bubble ">
              <v-card-text class="py-2 message-content">
                <div v-html="formattedContent(message.content)"></div>
                <div class="message-timestamp text-caption text-right mt-1">
                  {{ message.timestamp }}
                </div>
              </v-card-text>
            </v-card>
            <template v-slot:append v-if="message.direction === 'outgoing'">
              <AvatarDisplay :src="userProfileStore.userProfile?.avatar" :size="32" />
            </template>
          </v-list-item>
        </v-list>
      </v-card-text>
      <v-card-actions class="chat-input-container">
        <v-text-field v-model="newMessageText" :label="t('chat.inputPlaceholder')" variant="outlined" dense hide-details
          @keydown.enter="sendMessage" class="mr-2 text-black"></v-text-field>
        <v-btn color="primary" :loading="chatStore.isLoading" @click="sendMessage">
          <v-icon>mdi-send</v-icon>
        </v-btn>
      </v-card-actions>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue';
import { useChatStore } from '@/stores/chat.store';
import { useAuthStore } from '@/stores/auth.store';
import { useUserProfileStore } from '@/stores/userProfile.store';
import { useI18n } from 'vue-i18n';
import AvatarDisplay from '@/components/common/AvatarDisplay.vue';
import { Vue3Lottie } from 'vue3-lottie';
import ChatbotAnimation from '@/assets/json/chatbot.json';

const { t } = useI18n();

const chatOpen = ref(false);
const chatStore = useChatStore();
const authStore = useAuthStore();
const userProfileStore = useUserProfileStore();
const newMessageText = ref('');

const currentUserId = computed(() => authStore.user?.id || 'guest');

const toggleChat = () => {
  chatOpen.value = !chatOpen.value;
  if (chatOpen.value && !chatStore.selectedChatId) {
    chatStore.selectChat('ai-assistant', t);
  }
  if (chatOpen.value) {
    nextTick(() => {
      scrollToBottom();
    });
  }
};

const sendMessage = async () => {
  if (!newMessageText.value.trim() || chatStore.isLoading) return;

  const messageContent = newMessageText.value;
  newMessageText.value = '';

  await chatStore.sendMessage(messageContent, currentUserId.value);
  nextTick(() => {
    scrollToBottom();
  });
};

const scrollToBottom = () => {
  const messagesContainer = document.querySelector('.chat-messages-container');
  if (messagesContainer) {
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
  }
};

const formattedContent = (content: string) => {
  return content.replace(/\n/g, '<br>');
};

onMounted(() => {
  userProfileStore.fetchCurrentUserProfile();
  if (!chatStore.chatList.some(chat => chat.id === 'ai-assistant')) {
    chatStore.chatList.push({
      id: 'ai-assistant',
      name: 'AI Assistant',
      avatar: '',
      lastMessage: t('chat.initialMessage'),
      updatedAt: new Date().toLocaleTimeString(),
    });
  }
});
</script>

<style scoped>
.chat-widget-container {
  position: fixed;
  bottom: 15px;
  right: 15px;
  z-index: 1000;
}

.chat-window {
  width: 380px;
  height: 500px;
  display: flex;
  flex-direction: column;
  border-radius: 8px;
  overflow: hidden;
}

.chat-messages-container {
  flex-grow: 1;
  overflow-y: auto;
  padding: 5px;
  background-color: #f5f5f5;
}

.chat-messages-list {
  padding: 0;
  background: transparent;
}

.message-item {
  display: flex;
  margin-bottom: 4px;
}

.message-outgoing {
  justify-content: flex-end;
}

.message-incoming {
  justify-content: flex-start;
}

.message-bubble {
  border-radius: 20px;
  padding: 4px;
}

.message-outgoing .message-bubble {
  border-top-right-radius: 2px;

}

.message-incoming .message-bubble {
  border-top-left-radius: 2px;

}

.message-timestamp {
  font-size: 0.75em;
  opacity: 0.7;
  min-width: max-content;
  padding-right: 5px;
}

.chat-input-container {
  background-color: #fff;
  border-top: 1px solid #eee;
}

.message-content {
  white-space: pre-wrap;
  /* Preserve whitespace and line breaks */
}

.chat-bot-btn {
  cursor: pointer;
}
</style>

<style>
.chat-widget-container .v-list-item__prepend,
.chat-widget-container .v-list-item__append {
  align-self: start !important
}
</style>