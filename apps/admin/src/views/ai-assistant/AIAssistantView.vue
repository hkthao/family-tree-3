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
import type { GenerateFamilyDataCommand, CombinedAiContentDto, CardData, FamilyDto, MemberDto, EventDto } from '@/types';

interface Message {
  from: 'user' | 'ai';
  text?: string;
  cardData?: CardData[];
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

const services = useServices();
const chatService = services.chat;

const mapCombinedAiContentToCardData = (combinedContent: CombinedAiContentDto): CardData[] => {
  const cards: CardData[] = [];
  let idCounter = 1;

  combinedContent.families?.forEach((family: FamilyDto) => {
    cards.push({
      id: (idCounter++).toString(),
      type: 'Family',
      title: family.name,
      summary: family.description || ''
    });
  });

  combinedContent.members?.forEach((member: MemberDto) => {
    cards.push({
      id: (idCounter++).toString(),
      type: 'Member',
      title: `${member.firstName} ${member.lastName}`,
      summary: `${member.dateOfBirth ? new Date(member.dateOfBirth).getFullYear() : ''} - ${member.dateOfDeath ? new Date(member.dateOfDeath).getFullYear() : ''}`.trim()
    });
  });

  combinedContent.events?.forEach((event: EventDto) => {
    cards.push({
      id: (idCounter++).toString(),
      type: 'Event',
      title: event.name,
      summary: `${event.solarDate ? new Date(event.solarDate).toLocaleDateString() : ''} - ${event.description || event.name}`.trim()
    });
  });

  return cards;
};

const sendMessage = async () => {
  if (chatInput.value.trim()) {
    messages.value.push({ from: 'user', text: chatInput.value.trim() });
    const userMessage = chatInput.value.trim();
    chatInput.value = '';

    const command: GenerateFamilyDataCommand = {
      familyId: props.familyId,
      chatInput: userMessage,
    };

    const result = await chatService.generateFamilyData(command);

    if (result.ok && result.value) { // Changed to result.ok and result.value
      const cardData = mapCombinedAiContentToCardData(result.value); // Changed to result.value
      messages.value.push({ from: 'ai', cardData: cardData });
    } else if (!result.ok) { // Added else if for error handling
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