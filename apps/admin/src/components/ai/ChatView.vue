<template>
  <v-card class="mx-auto d-flex flex-column chat-card" flat :loading="loading" height="100%">
    <v-card-title class="d-flex align-center justify-center">
      <v-icon left>mdi-robot-outline</v-icon>
      {{ t('aiChat.title') }}
    </v-card-title>
    <v-card-text class="flex-grow-1 pa-0">
      <v-sheet class="pa-4 h-100 overflow-y-auto pb-16">
        <div class="chat-messages" ref="chatMessagesContainer">
          <div v-for="(message, index) in messages" :key="index"
            :class="['d-flex', message.sender === 'user' ? 'justify-end' : 'justify-start']">
            <v-chip class="ma-1" :color="message.sender === 'user' ? 'primary' : 'grey-lighten-1'" label>
              <div v-if="message.sender === 'ai'">
                <v-icon start icon="mdi-robot-outline"></v-icon>
              </div>
              <div v-else>
                <v-icon start icon="mdi-account-circle"></v-icon>
              </div>
              {{ message.text }}
            </v-chip>
          </div>
          <div v-if="loading" class="d-flex justify-start">
            <v-chip class="ma-1" color="grey-lighten-1" label>
              <v-progress-circular indeterminate size="20" width="2"></v-progress-circular>
              <span class="ml-2">{{ t('aiChat.typing') }}</span>
            </v-chip>
          </div>
        </div>
      </v-sheet>
    </v-card-text>
    <v-card-actions class="d-flex justify-center pa-4 chat-input-actions">
      <v-textarea counter :rows="2" v-model="newMessage" :placeholder="t('aiChat.placeholder')" variant="outlined"
        @keyup.enter="sendMessage" :disabled="loading">
        <template v-slot:append-inner>
          <v-btn icon flat :disabled="loading || !newMessage.trim()" @click="sendMessage">
            <v-icon>mdi-send</v-icon>
          </v-btn>
        </template>
      </v-textarea>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
// Assuming a composable for AI chat functionality
import { useAiChat } from '@/composables/ai/aiChat.composable';

const props = defineProps<{
  familyId: string;
}>();

const { t } = useI18n();
const newMessage = ref('');
const chatMessagesContainer = ref<HTMLElement | null>(null);

const { messages, loading, sendMessage: sendAiMessage } = useAiChat(props.familyId);

const sendMessage = async () => {
  if (newMessage.value.trim() && !loading.value) {
    const messageText = newMessage.value;
    newMessage.value = '';
    await sendAiMessage(messageText);
    scrollToBottom();
  }
};

const scrollToBottom = () => {
  nextTick(() => {
    if (chatMessagesContainer.value) {
      chatMessagesContainer.value.scrollTop = chatMessagesContainer.value.scrollHeight;
    }
  });
};

// Scroll to bottom when messages change
watch(messages, () => {
  scrollToBottom();
}, { deep: true });

</script>

<style scoped>
.chat-card {
  position: relative;
  min-height: calc(100vh - 250px);
}

.chat-input-actions {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  width: 100%;
  background-color: var(--v-card-background);
}
</style>
