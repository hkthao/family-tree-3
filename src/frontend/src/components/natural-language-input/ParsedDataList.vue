<template>
  <v-row v-if="parsedResult">
    <!-- Members Column - Displays 2 columns on medium screens and up, 1 column on smaller screens -->
    <v-col cols="12">
      <h3 class="mb-2">{{ t('naturalLanguage.editor.parsedMembers') }}</h3>
      <ParsedResultCard v-for="(member, index) in parsedResult.members" :key="`member-${index}`" :item="member"
        type="member" @delete="deleteMember(index)" />
    </v-col>

    <!-- Events Column - Displays 2 columns on medium screens and up, 1 column on smaller screens -->
    <v-col cols="12">
      <h3 class="mb-2">{{ t('naturalLanguage.editor.parsedEvents') }}</h3>
      <ParsedResultCard v-for="(event, index) in parsedResult.events" :key="`event-${index}`" :item="event" type="event"
        @delete="deleteEvent(index)" />
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import ParsedResultCard from './ParsedResultCard.vue';
import type { AnalyzedDataDto } from '@/types/natural-language.d';
import { useI18n } from 'vue-i18n';

defineProps<{
  parsedResult: AnalyzedDataDto | null;
}>();

const emit = defineEmits(['delete-member', 'delete-event']); // Define new emits

const { t } = useI18n();

const deleteMember = (index: number) => {
  emit('delete-member', index); // Emit event instead of direct mutation
};

const deleteEvent = (index: number) => {
  emit('delete-event', index); // Emit event instead of direct mutation
};
</script>
