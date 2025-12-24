<template>
  <v-container fluid class="pa-0">
    <v-row>
      <v-col cols="12">
        <v-alert type="info" :text="t('aiAssistant.infoMessage')" variant="tonal" class="mb-4"></v-alert>
        <v-sheet class="chat-messages" color="transparent">
          <div v-for="(message, index) in messages" :key="index"
            :class="['d-flex', message.from === 'user' ? 'justify-end' : 'justify-start']">
            <template v-if="message.text">
              <v-chip :color="message.from === 'user' ? 'primary' : 'grey'" class="ma-1">
                {{ message.text }}
              </v-chip>
            </template>
            <template v-else-if="message.cardData">
              <div :class="['d-flex', 'flex-row ga-2 flex-wrap', message.from === 'user' ? 'align-end' : 'align-start']">
                <AICard
                  v-for="card in message.cardData"
                  :key="card.id"
                  :card="card"
                  :from="message.from"
                  @save="handleSaveCard"
                  @delete="handleDeleteCard"
                />
              </div>
            </template>
          </div>
        </v-sheet>
        <v-textarea v-model="chatInput" :label="t('aiAssistant.inputLabel')" :hint="t('aiAssistant.inputHint')"
          persistent-hint variant="outlined" rows="2" :auto-grow="false" hide-details :no-resize="true"
          @keydown.enter.prevent="sendMessage">
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
import AICard from '@/components/chat-generated-cards/AICard.vue';
import { useServices } from '@/plugins/services.plugin';
import { GenerateAiContentCommand } from '@/types'; // Import for the command type

interface CardData {
  id: string; // Assuming each card needs a unique ID for actions
  type: string;
  title: string;
  summary: string;
}

interface Message {
  from: 'user' | 'ai';
  text?: string; // Make text optional if cardData is present
  cardData?: CardData[]; // New property for card data
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

const services = useServices(); // Get all services
const chatService = services.chat; // Get the chat service

const sendMessage = async () => { // Make it async
  if (chatInput.value.trim()) {
    messages.value.push({ from: 'user', text: chatInput.value.trim() });
    const userMessage = chatInput.value.trim(); // Store message before clearing input
    chatInput.value = ''; // Clear input immediately

    // Define the command for AI content generation
    const command: GenerateAiContentCommand = {
      familyId: props.familyId, // Use the familyId prop
      chatInput: userMessage,
      contentType: 'Member' // For now, assume 'Member' as a default content type. This might need to be dynamic later.
    };

    // Call the new API method
    const result = await chatService.generateAiContent(command);

    if (result.success && result.data) {
      messages.value.push({ from: 'ai', cardData: result.data });
    } else {
      // Handle error, e.g., display an error message
      messages.value.push({ from: 'ai', text: `Error: ${result.error?.message || 'Failed to generate content.'}` });
    }
  }
};


const handleSaveCard = (id: string) => {
  console.log('Saving card:', id);
  // Implement actual save logic here
};

const handleDeleteCard = (id: string) => {
  console.log('Deleting card:', id);
  // Implement actual delete logic here
};
</script>
<style scoped>
.chat-messages{
  margin-bottom: 15px;
  max-height: calc(100vh - 440px);
  overflow-y: auto;
  border-radius: 8px;
}

</style>