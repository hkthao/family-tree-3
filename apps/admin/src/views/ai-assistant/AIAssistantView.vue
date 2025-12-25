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

    <!-- Add Event Drawer -->
    <BaseCrudDrawer v-model="addEventDrawer" @close="handleEventClosed">
      <EventAddView v-if="addEventDrawer" :family-id="props.familyId"
        :initial-event-data="eventDataToAdd"
        @close="handleEventClosed" @saved="handleEventSaved"
        :allow-save="true" />
    </BaseCrudDrawer>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import AICard from '@/components/chat-generated-cards/AICard.vue';

import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import EventAddView from '@/views/event/EventAddView.vue'; // Added EventAddView
import { useAiAssistant } from '@/composables/ai-assistant/useAiAssistant';

const props = defineProps({
  familyId: {
    type: String,
    required: true,
  },
});

const { t } = useI18n();
const { state, actions } = useAiAssistant({ familyId: props.familyId });

// Destructure state and actions for easier template binding
const { chatInput, messages, isLoadingAiResponse, addMemberDrawer, memberDataToAdd, addEventDrawer, eventDataToAdd } = state;
const { sendMessage, handleSaveCard, handleMemberClosed, handleMemberSaved, handleDeleteCard, handleEventClosed, handleEventSaved } = actions;
</script>

<style scoped>
.chat-messages{
  margin-bottom: 15px;
  max-height: calc(100vh - 440px);
  overflow-y: auto;
  border-radius: 8px;
}

</style>