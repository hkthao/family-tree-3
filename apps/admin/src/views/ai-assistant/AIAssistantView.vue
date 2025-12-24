<template>
  <v-container fluid class="pa-0">
    <v-row no-gutters>
      <v-col cols="12">
        <v-card flat>
          <v-card-text>
            <div class="chat-messages" style="height: 400px; overflow-y: auto; border: 1px solid #ccc; padding: 10px;">
              <!-- Chat messages will go here -->
              <div v-for="(message, index) in messages" :key="index"
                :class="['d-flex', message.from === 'user' ? 'justify-end' : 'justify-start']">
                <v-chip :color="message.from === 'user' ? 'primary' : 'grey'" class="ma-1">
                  {{ message.text }}
                </v-chip>
              </div>
            </div>
            <v-row class="mt-4" no-gutters>
              <v-col cols="10">
                <v-text-field v-model="chatInput" :label="t('aiAssistant.inputPlaceholder')" variant="outlined" density="compact"
                  hide-details @keyup.enter="sendMessage"></v-text-field>
              </v-col>
              <v-col cols="2" class="d-flex align-center justify-center">
                <v-btn :disabled="!chatInput.trim()" icon color="primary" @click="sendMessage">
                  <v-icon>mdi-send</v-icon>
                </v-btn>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
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

<style scoped>
.chat-messages {
  max-height: 70vh;
  overflow-y: auto;
  padding-right: 10px; /* To prevent scrollbar from overlapping text */
}
</style>
