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
          <div v-if="isLoadingAiResponse" class="d-flex justify-start">
            <v-chip class="ma-1">
              <v-progress-circular indeterminate size="20" width="2" class="mr-2"></v-progress-circular>
              <span>{{ t('aiChat.typing') }}</span>
            </v-chip>
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

    <!-- Add Member Drawer -->
    <BaseCrudDrawer v-model="addMemberDrawer" @close="handleMemberClosed">
      <MemberAddView v-if="addMemberDrawer" :family-id="props.familyId"
        :initial-member-data="memberDataToAdd"
        @close="handleMemberClosed" @saved="handleMemberSaved"
        :allow-save="true" />
    </BaseCrudDrawer>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import AICard from '@/components/chat-generated-cards/AICard.vue';
import { useServices } from '@/plugins/services.plugin';
import { useConfirmDialog } from '@/composables/ui/useConfirmDialog';
import type { GenerateFamilyDataDto, CombinedAiContentDto, CardData, MemberDto, EventDto } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useQueryClient } from '@tanstack/vue-query';

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
const isLoadingAiResponse = ref(false);
const addMemberDrawer = ref(false); // Ref to control the MemberAddView drawer
const memberDataToAdd = ref<MemberDto | null>(null); // Ref to hold member data to add
const savingCardId = ref<string | null>(null); // New ref to store the id of the card being saved
const queryClient = useQueryClient(); // Initialize useQueryClient
const { showSnackbar } = useGlobalSnackbar(); // Initialize useGlobalSnackbar

const services = useServices();
const chatService = services.chat;
const { showConfirmDialog } = useConfirmDialog();

const mapCombinedAiContentToCardData = (combinedContent: CombinedAiContentDto): CardData[] => {
  const cards: CardData[] = [];
  let idCounter = 1;

  combinedContent.members?.forEach((member: MemberDto) => {
    cards.push({
      id: (idCounter++).toString(),
      type: 'Member',
      title: `${member.firstName} ${member.lastName}`,
      summary: `${member.dateOfBirth ? new Date(member.dateOfBirth).getFullYear() : ''} - ${member.dateOfDeath ? new Date(member.dateOfDeath).getFullYear() : ''}`.trim(),
      data: {
        ...member,
        familyId: props.familyId, // Default to props.familyId if not provided by AI
        dateOfBirth: member.dateOfBirth ? new Date(member.dateOfBirth) : undefined,
        dateOfDeath: member.dateOfDeath ? new Date(member.dateOfDeath) : undefined,
      } as MemberDto, // Store the full member object with parsed dates
    });
  });

  combinedContent.events?.forEach((event: EventDto) => {
    cards.push({
      id: (idCounter++).toString(),
      type: 'Event',
      title: event.name,
      summary: `${event.solarDate ? new Date(event.solarDate).toLocaleDateString() : ''} - ${event.description || event.name}`.trim(),
      data: {
        ...event,
        familyId: props.familyId, // Default to props.familyId if not provided by AI
        solarDate: event.solarDate ? new Date(event.solarDate) : undefined,
      } as EventDto, // Store the full event object with parsed dates
    });
  });

  return cards;
};

const sendMessage = async () => {
  if (chatInput.value.trim()) {
    messages.value.push({ from: 'user', text: chatInput.value.trim() });
    const userMessage = chatInput.value.trim();
    chatInput.value = '';

    isLoadingAiResponse.value = true; // Set loading to true

    try {
      const command: GenerateFamilyDataDto = {
        familyId: props.familyId,
        chatInput: userMessage,
      };

      const result = await chatService.generateFamilyData(command);

      if (result.ok && result.value) {
        const cardData = mapCombinedAiContentToCardData(result.value);
        messages.value.push({ from: 'ai', cardData: cardData });
      } else if (!result.ok) {
        messages.value.push({ from: 'ai', text: `Error: ${result.error?.message || 'Failed to generate content.'}` });
      }
    } finally {
      isLoadingAiResponse.value = false; // Set loading to false
    }
  }
};

const handleSaveCard = (id: string) => {
  // Find the card in the messages
  let foundCard: CardData | undefined;
  for (const message of messages.value) {
    if (message.cardData) {
      foundCard = message.cardData.find(card => card.id === id);
      if (foundCard) {
        break;
      }
    }
  }

  if (foundCard) {
    if (foundCard.type === 'Member') {
      memberDataToAdd.value = foundCard.data as MemberDto;
      savingCardId.value = id; // Store the id of the card being saved
      addMemberDrawer.value = true;
    } else if (foundCard.type === 'Family') {
      showSnackbar(t('aiAssistant.messages.familySaveNotImplemented'), 'info');
    } else if (foundCard.type === 'Event') {
      showSnackbar(t('aiAssistant.messages.eventSaveNotImplemented'), 'info');
    }
  }
};

const handleMemberClosed = () => {
  addMemberDrawer.value = false;
  memberDataToAdd.value = null;
  savingCardId.value = null; // Clear the saving card id
};

const handleMemberSaved = () => {
  addMemberDrawer.value = false;
  memberDataToAdd.value = null;

  // Mark the saved card as isSaved = true
  if (savingCardId.value) {
    messages.value.forEach(message => {
      if (message.cardData) {
        const card = message.cardData.find(c => c.id === savingCardId.value);
        if (card) {
          card.isSaved = true;
        }
      }
    });
    savingCardId.value = null; // Clear the saving card id
  }

  queryClient.invalidateQueries({ queryKey: ['members', 'list'] }); // Invalidate member list to refetch
  showSnackbar(t('member.messages.addSuccess'), 'success');
};

const handleDeleteCard = async (id: string) => {
  const result = await showConfirmDialog({
    title: t('common.confirm'),
    message: t('aiAssistant.confirmDeleteCard'),
    confirmText: t('common.confirm'),
    cancelText: t('common.cancel'),
    color: 'error',
  });

  if (result) {
    messages.value.forEach(message => {
      if (message.from === 'ai' && message.cardData) {
        message.cardData = message.cardData.filter(card => card.id !== id);
      }
    });
  }
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