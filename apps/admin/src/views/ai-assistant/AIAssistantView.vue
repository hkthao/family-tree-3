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
          show-size @change="handleFileChangeWrapper" style="display: none;"></v-file-input>
        <v-form ref="formRef" v-model="formIsValid">
          <v-textarea v-model="state.chatInput" :label="t('aiAssistant.inputLabel')" :hint="t('aiAssistant.inputHint')"
            persistent-hint variant="outlined" rows="2" :max-rows="10" :auto-grow="true" :no-resize="true"
            :hide-details="false" maxlength="2000" counter :rules="validationRules.chatInput"
            @keydown.enter.prevent="sendMessage"> <template v-slot:prepend-inner>
              <v-btn variant="text" icon color="primary" @click="triggerFileInput" class="mr-2">
                <v-icon>mdi-ocr</v-icon> <!-- Icon for OCR -->
                <v-tooltip activator="parent" location="top">{{ t('aiAssistant.processOcr') }}</v-tooltip>
              </v-btn>
            </template>

            <template v-slot:append-inner>
              <v-btn :disabled="!formIsValid" variant="text" icon color="primary" @click="sendMessage">
                <v-icon>mdi-send</v-icon>
              </v-btn>
            </template>
          </v-textarea>
        </v-form> </v-col>
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
import { ref } from 'vue';
import { useI18n } from 'vue-i18n'; // ADDED
import AICard from '@/components/chat-generated-cards/AICard.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import { useAiAssistant } from '@/composables/ai-assistant/useAiAssistant';
import { useAiAssistantRules } from '@/validations/ai-assistant.validation'; // Import the new validation composable

const props = defineProps({
  familyId: {
    type: String,
    required: true,
  },
});

const { t } = useI18n();
const { state, actions } = useAiAssistant({ familyId: props.familyId });

// Initialize validation rules
const validationRules = useAiAssistantRules({ chatInput: state.chatInput });

// Refs for form and file input
const formRef = ref<HTMLFormElement | null>(null);
const fileInputRef = ref<HTMLInputElement | null>(null);

// Form validity state
const formIsValid = ref(false);

// Destructure state and actions for easier template binding
const { messages, isLoadingAiResponse, addMemberDrawer, memberDataToAdd, addEventDrawer, eventDataToAdd, selectedFile } = state;
const { sendMessage: originalSendMessage, handleSaveCard, handleMemberClosed, handleMemberSaved, handleDeleteCard, handleEventClosed, handleEventSaved, handleFileChange, clearFileInput } = actions;

// Function to trigger the file input click
const triggerFileInput = () => {
  if (fileInputRef.value) {
    fileInputRef.value.click();
  }
};

// Wrapper function to handle file change and clear the input
const handleFileChangeWrapper = async (event: Event) => {
  await handleFileChange(event); // Call the composable's handleFileChange
  clearFileInput(fileInputRef.value); // Clear the native file input element
};

// Override sendMessage to include form validation
const sendMessage = async () => {
  if (formRef.value) {
    const { valid } = await formRef.value.validate();
    if (valid) {
      await originalSendMessage();
    }
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