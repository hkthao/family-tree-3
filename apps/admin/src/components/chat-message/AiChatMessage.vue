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
        color="primary"
        variant="elevated"
        size="small"
        append-icon="mdi-open-in-new"
        @click="navigateToRelationshipPage"
      >
        {{ t('aiChat.determineRelationship') }}
      </v-btn>
    </template>
  </v-sheet>
</template>

<script setup lang="ts">
import type { PropType } from 'vue'; // Use type import
import { useRouter } from 'vue-router';
import type { AiChatMessage } from '@/types/chat.d'; // Use type import
import { useI18n } from 'vue-i18n';

const props = defineProps({
  message: {
    type: Object as PropType<AiChatMessage>,
    required: true,
  },
  familyId: {
    type: String,
    required: true,
  },
});

const router = useRouter();
const { t } = useI18n();

const navigateToRelationshipPage = () => {
  // Construct the URL using the familyId and the intent from the message
  // The backend currently sends "RELATIONSHIP_LOOKUP_PAGE" as the intent.
  // The frontend needs to map this to an actual route.
  // Assuming a route like '/family/:familyId/relationships'
  router.push(`/family/${props.familyId}/relationships`);
};
</script>

<style scoped>
.message-content {
  white-space: pre-wrap; /* Preserves whitespace and wraps text */
  word-break: break-word; /* Ensures long words break to prevent overflow */
}
</style>