<template>
  <v-container fluid class="pa-0">
    <v-row>
      <v-col cols="12">
        <v-sheet class="chat-messages" color="transparent" height="calc(100vh - 270px)"
          max-height="calc(100vh - 270px)">
          <div v-for="(message, index) in messages" :key="index"
            :class="['d-flex', message.from === 'user' ? 'justify-end' : 'justify-start']">
            <v-chip :color="message.from === 'user' ? 'primary' : 'grey'" class="ma-1">
              {{ message.text }}
            </v-chip>
          </div>
        </v-sheet>
        <v-textarea v-model="chatInput" :label="t('aiAssistant.inputPlaceholder')" variant="outlined" rows="2" auto-grow
          hide-details @keydown.enter.prevent="sendMessage">
          <template v-slot:append-inner>
            <v-btn :disabled="!chatInput.trim()" variant="text" icon color="primary" @click="sendMessage">
              <v-icon>mdi-send</v-icon>
            </v-btn>
          </template>
        </v-textarea>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

interface Message {
  from: 'user' | 'ai';
  text: string;
}

const props = defineProps({
  familyId: {
    type: String,
    required: true,
  },
});

const { t } = useI18n();
const chatInput = ref('');
const messages = ref<Message[]>([]);

const sendMessage = () => {
  if (chatInput.value.trim()) {
    messages.value.push({ from: 'user', text: chatInput.value.trim() });
    // Simulate AI response
    setTimeout(() => {
      messages.value.push({ from: 'ai', text: `AI: You said "${chatInput.value.trim()}"` });
    }, 1000);
    chatInput.value = '';
  }
};
</script>
