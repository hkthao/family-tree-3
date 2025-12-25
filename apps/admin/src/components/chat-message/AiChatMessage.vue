<template>
  <v-avatar class="mr-1" size="36">
    <v-icon>mdi-robot-outline</v-icon>
  </v-avatar>
  <v-sheet class="ma-1 pa-2 text-wrap" color="secondary" rounded="lg">
    <div class="message-content">
      {{ message.text }}
    </div>
    <template v-if="message.intent === 'RELATIONSHIP_LOOKUP_PAGE'">
      <v-btn
        class="mt-2"
        variant="outlined"
        size="small"
        append-icon="mdi-arrow-right-circle"
        @click="emit('open-relationship-detection', familyId)"
      >
        {{ t('aiChat.determineRelationship') }}
      </v-btn>
    </template>
  </v-sheet>
</template>

<script setup lang="ts">
import type { PropType } from 'vue'; // Use type import
import type { AiChatMessage } from '@/types/chat.d'; // Use type import
import { useI18n } from 'vue-i18n';

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

const emit = defineEmits(['open-relationship-detection']);
const { t } = useI18n();
</script>

<style scoped>
.message-content {
  white-space: pre-wrap; /* Preserves whitespace and wraps text */
  word-break: break-word; /* Ensures long words break to prevent overflow */
}
</style>
