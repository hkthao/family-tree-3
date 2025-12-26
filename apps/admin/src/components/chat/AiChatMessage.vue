<template>
  <v-avatar class="mr-1" size="36">
    <v-icon>mdi-robot-outline</v-icon>
  </v-avatar>
  <v-sheet class="ma-1 pa-2 text-wrap v-sheet-message" color="secondary" rounded="lg">
    <div class="message-content">
      {{ message.text }}
    </div>
    <template v-if="message.intent === 'RELATIONSHIP_LOOKUP_PAGE'">
      <v-btn class="mt-2" variant="outlined" size="small" append-icon="mdi-arrow-right-circle"
        @click="emit('open-relationship-detection', familyId)">
        {{ t('aiChat.determineRelationship') }}
      </v-btn>
    </template>

    <ChatGeneratedDataList
      v-if="message.generatedData && (message.generatedData.members.length > 0 || message.generatedData.events.length > 0)"
      :generatedData="message.generatedData" :familyId="familyId"
      @add-generated-member="(member: MemberDto) => emit('add-generated-member', member)"
      @add-generated-event="(event: EventDto) => emit('add-generated-event', event)" />

    <AiFaceRecognitionDisplay
      v-if="message.intent === 'IMAGE_RECOGNITION_PAGE'"
      :message="message"
      :familyId="familyId"
    />
  </v-sheet>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import type { AiChatMessage, EventDto, MemberDto } from '@/types';
import { useI18n } from 'vue-i18n';
import ChatGeneratedDataList from '@/components/chat/ChatGeneratedDataList.vue';
import AiFaceRecognitionDisplay from '@/components/chat/AiFaceRecognitionDisplay.vue'; // NEW import

defineProps({
  message: {
    type: Object as PropType<AiChatMessage>,
    required: true,
  },
  familyId: {
    type: String,
    required: true,
  },
});

const emit = defineEmits([
  'open-relationship-detection',
  'add-generated-member',
  'add-generated-event',
]);
const { t } = useI18n();
</script>

<style scoped>
.message-content {
  white-space: pre-wrap;
  word-break: break-word;
}

.v-sheet-message {
  max-width: 89%;
}
</style>
