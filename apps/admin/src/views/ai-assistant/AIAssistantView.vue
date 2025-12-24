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

const sendMessage = () => {
  if (chatInput.value.trim()) {
    messages.value.push({ from: 'user', text: chatInput.value.trim() });
    // Simulate AI response
    setTimeout(() => {
      const mockCardData: CardData[] = [
        { id: '1', type: 'Member', title: 'Nguyễn Văn A', summary: 'Ông nội, sinh 1945, mất 2002' },
        { id: '2', type: 'Relationship', title: 'Trần Thị B - Cô ruột', summary: 'Sinh khoảng 1970' },
        { id: '3', type: 'Family', title: 'Trần Thị B - Cô ruột', summary: 'Sinh khoảng 1970' },
        { id: '4', type: 'Relationship', title: 'Trần Thị B - Cô ruột', summary: 'Sinh khoảng 1970' },
        { id: '5', type: 'Relationship', title: 'Trần Thị B - Cô ruột', summary: 'Sinh khoảng 1970' },
      ];
      messages.value.push({ from: 'ai', cardData: mockCardData });
    }, 1000);
    chatInput.value = '';
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