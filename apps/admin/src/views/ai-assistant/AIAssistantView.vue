<template>
  <v-container fluid class="pa-0">
    <v-row>
      <v-col cols="12">
        <v-alert type="info" :text="t('aiAssistant.infoMessage')" variant="tonal" class="mb-4"></v-alert>
        <v-sheet class="chat-messages" color="transparent">
          <div v-for="(message, index) in messages" :key="index"
            :class="['d-flex', message.from === 'user' ? 'justify-end' : 'justify-start']">
            <template v-if="message.text">
              <v-sheet :color="message.from === 'user' ? 'primary' : 'grey'" class="my-1 pa-2" border rounded>
                {{ message.text }}
              </v-sheet>
            </template>
            <template v-else-if="message.cardData">
              <div
                :class="['d-flex', 'flex-row ga-2 flex-wrap', message.from === 'user' ? 'align-end' : 'align-start']">
                <AICard v-for="card in message.cardData" :key="card.id" :card="card" :from="message.from"
                  @save="handleSaveCard" @delete="handleDeleteCard" />
              </div>
            </template>
          </div>
          <div v-if="isLoadingAiResponse" class="d-flex justify-start">
            <v-sheet class="my-1 pa-2" border rounded>
              <v-progress-circular indeterminate size="20" width="2" class="mr-2"></v-progress-circular>
              <span>{{ t('aiChat.typing') }}</span>
            </v-sheet>
          </div>
        </v-sheet>
        <v-file-input ref="fileInputRef" v-model="selectedFile" :label="t('aiAssistant.uploadFileLabel')"
          density="compact" prepend-icon="" append-icon="mdi-paperclip" accept="image/*,application/pdf" hide-details
          show-size @change="handleFileChange" style="display: none;"></v-file-input>
        <v-textarea v-model="state.chatInput" :label="t('aiAssistant.inputLabel')" :hint="t('aiAssistant.inputHint')"
          persistent-hint variant="outlined" rows="2" :auto-grow="false" hide-details :no-resize="true"
          @keydown.enter.prevent="sendMessage">
          <template v-slot:prepend-inner>
            <v-btn variant="text" icon color="primary" @click="triggerFileInput" class="mr-2">
              <v-icon>mdi-ocr</v-icon> <!-- Icon for OCR -->
              <v-tooltip activator="parent" location="top">{{ t('aiAssistant.processOcr') }}</v-tooltip>
            </v-btn>
          </template>

          <template v-slot:append-inner>
            <v-btn :disabled="!state.chatInput.trim()" variant="text" icon color="primary" @click="sendMessage">
              <v-icon>mdi-send</v-icon>
            </v-btn>
          </template>
        </v-textarea>
      </v-col>
    </v-row>

    <!-- Add Member Drawer -->
    <BaseCrudDrawer v-model="addMemberDrawer" @close="handleMemberClosed">
      <MemberAddView v-if="addMemberDrawer" :family-id="props.familyId" :initial-member-data="memberDataToAdd"
        @close="handleMemberClosed" @saved="handleMemberSaved" :allow-save="true" />
    </BaseCrudDrawer>

    <!-- Add Event Drawer -->
    <BaseCrudDrawer v-model="addEventDrawer" @close="handleEventClosed">
      <EventAddView v-if="addEventDrawer" :family-id="props.familyId" :initial-event-data="eventDataToAdd"
        @close="handleEventClosed" @saved="handleEventSaved" :allow-save="true" />
    </BaseCrudDrawer>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import AICard from '@/components/chat-generated-cards/AICard.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import { useAiAssistant } from '@/composables/ai-assistant/useAiAssistant';
import { ref } from 'vue'; // Import ref

const props = defineProps({
  familyId: {
    type: String,
    required: true,
  },
});

const { t } = useI18n();
const { state, actions } = useAiAssistant({ familyId: props.familyId });

// Destructure state and actions for easier template binding
const { messages, isLoadingAiResponse, addMemberDrawer, memberDataToAdd, addEventDrawer, eventDataToAdd, selectedFile } = state;
const { sendMessage, handleSaveCard, handleMemberClosed, handleMemberSaved, handleDeleteCard, handleEventClosed, handleEventSaved, handleFileChange } = actions;

// Ref for the file input
const fileInputRef = ref<HTMLInputElement | null>(null);

// Function to trigger the file input click
const triggerFileInput = () => {
  if (fileInputRef.value) {
    fileInputRef.value.click();
  }
};
</script>

<style scoped>
.chat-messages {
  margin-bottom: 15px;
  max-height: calc(100vh - 440px);
  overflow-y: auto;
  border-radius: 8px;
}
</style>