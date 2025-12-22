<template>
  <v-card class="mx-auto d-flex flex-column chat-card" flat :loading="state.loading.value" height="100%">
    <v-card-text class="flex-grow-1 pa-0">
      <v-sheet class="pa-4 h-100 overflow-y-auto pb-32">
        <div class="chat-messages" ref="chatMessagesContainer">
          <div v-for="(message, index) in state.messages" :key="index"
            :class="['d-flex align-center my-1', message.sender === 'user' ? 'justify-end' : 'justify-start']">
            <template v-if="message.sender === 'user'">
              <v-chip class="ma-1" color="primary" label>
                {{ message.text }}
              </v-chip>
              <v-avatar cover class="ml-1" size="36">
                <v-img v-if="state.userProfile?.value?.avatar"
                  :src="getAvatarUrl(state.userProfile.value.avatar, undefined)"
                  :alt="state.userProfile.value.name || 'User'" />
                <v-icon v-else>mdi-account-circle</v-icon>
              </v-avatar>
            </template>
            <template v-else>
              <v-avatar class="mr-1" size="36">
                <v-icon>mdi-robot-outline</v-icon>
              </v-avatar>
              <v-chip class="ma-1" color="grey-lighten-1" label>
                {{ message.text }}
              </v-chip>
            </template>
            <template v-if="!message.text">
              <!-- Debugging: log message if text is empty or not a string -->
              {{ console.log('Message without text property:', message) }}
            </template>
          </div>
          <div v-if="state.loading.value" class="d-flex justify-start">
            <v-chip class="ma-1" color="grey-lighten-1" label>
              <v-progress-circular indeterminate size="20" width="2"></v-progress-circular>
              <span class="ml-2">{{ t('aiChat.typing') }}</span>
            </v-chip>
          </div>
        </div>
      </v-sheet>
    </v-card-text>
    <!-- Suggestion Chips -->
    <div class="chat-suggestions-container px-4">
      <v-chip-group column>
        <v-chip v-for="(suggestion, index) in state.suggestionChips" :key="index"
          @click="actions.selectSuggestion(suggestion)">
          {{ suggestion }}
        </v-chip>
      </v-chip-group>
    </div>

    <v-card-actions class="d-flex justify-center pa-4 chat-input-actions">
      <v-textarea no-resize :auto-grow="false" counter :rows="2"
        :model-value="state.newMessage.value"
        @update:model-value="newValue => state.newMessage.value = newValue"
        :placeholder="t('aiChat.placeholder')" variant="outlined" @keyup.enter="actions.sendMessage"
        :disabled="state.loading.value">
        <template v-slot:append-inner>
          <v-btn icon flat :disabled="state.loading.value || !state.newMessage.value.trim()" @click="actions.sendMessage">
            <v-icon>mdi-send</v-icon>
          </v-btn>
        </template>
      </v-textarea>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'; // Added onMounted for debugging
import { useI18n } from 'vue-i18n'; // Keep useI18n for `t` function in template
import { getAvatarUrl } from '@/utils/avatar.utils'; // Keep getAvatarUrl as it's a utility

import { useChatView } from '@/composables/ai/useChatView'; // Import the new composable

const props = defineProps<{
  familyId: string;
}>();

const { t } = useI18n(); // Keep t for template

const chatMessagesContainer = ref<HTMLElement | null>(null);

const { state, actions } = useChatView(props.familyId);

const scrollToBottom = () => {
  nextTick(() => {
    if (chatMessagesContainer.value) {
      chatMessagesContainer.value.scrollTop = chatMessagesContainer.value.scrollHeight;
    }
  });
};

// Scroll to bottom when messages change
watch(state.messages, () => {
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

.chat-suggestions-container {
  position: absolute;
  bottom: 100px;
  /* Adjust based on input field height */
  left: 0;
  right: 0;
  width: 100%;
  z-index: 1;
  /* Ensure chips are above other content */
}
</style>
