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

    <template v-if="message.generatedData && (message.generatedData.members.length > 0 || message.generatedData.events.length > 0)">
      <v-card class="mt-4" flat variant="outlined">
        <v-card-title class="pa-2">{{ t('aiChat.generatedData') }}</v-card-title>
        <v-list density="compact">
          <template v-for="(member, index) in message.generatedData.members" :key="`member-${index}`">
            <v-list-item>
              <template v-slot:prepend>
                <v-icon>mdi-account</v-icon>
              </template>
              <v-list-item-title>{{ member.fullName }}</v-list-item-title>
              <v-list-item-subtitle>
                {{ member.gender === Gender.Male ? t('common.male') : t('common.female') }}
                <template v-if="member.dateOfBirth">, {{ t('common.born') }} {{ new Date(member.dateOfBirth).getFullYear() }}</template>
                <template v-if="member.dateOfDeath">, {{ t('common.died') }} {{ new Date(member.dateOfDeath).getFullYear() }}</template>
              </v-list-item-subtitle>
              <template v-slot:append>
                <v-btn icon size="small" variant="text" @click="emit('add-generated-member', member)">
                  <v-icon>mdi-plus-circle-outline</v-icon>
                </v-btn>
              </template>
            </v-list-item>
            <v-divider v-if="index < message.generatedData.members.length - 1 || message.generatedData.events.length > 0"></v-divider>
          </template>

          <template v-for="(event, index) in message.generatedData.events" :key="`event-${index}`">
            <v-list-item>
              <template v-slot:prepend>
                <v-icon>mdi-calendar-month</v-icon>
              </template>
              <v-list-item-title>{{ event.name }}</v-list-item-title>
              <v-list-item-subtitle>
                <template v-if="event.description">{{ event.description }}</template>
                <template v-if="event.solarDate">, {{ t('common.date') }} {{ new Date(event.solarDate).toLocaleDateString() }}</template>
              </v-list-item-subtitle>
              <template v-slot:append>
                <v-btn icon size="small" variant="text" @click="emit('add-generated-event', event)">
                  <v-icon>mdi-plus-circle-outline</v-icon>
                </v-btn>
              </template>
            </v-list-item>
            <v-divider v-if="index < message.generatedData.events.length - 1"></v-divider>
          </template>
        </v-list>
      </v-card>
    </template>
  </v-sheet>
</template>

<script setup lang="ts">
import type { PropType } from 'vue'; // Use type import
import type { AiChatMessage } from '@/types';
import { Gender } from '@/types/member.d'; // Import Gender enum
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

const emit = defineEmits([
  'open-relationship-detection',
  'add-generated-member', // New event
  'add-generated-event',  // New event
]);
const { t } = useI18n();
</script>

<style scoped>
.message-content {
  white-space: pre-wrap; /* Preserves whitespace and wraps text */
  word-break: break-word; /* Ensures long words break to prevent overflow */
}
</style>
