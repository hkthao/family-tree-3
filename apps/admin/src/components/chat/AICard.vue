<template>
  <v-card variant="tonal" compact width="250">
    <v-card-title class="text-h6">
      <v-icon start :icon="getIconForType(card.type)"></v-icon>
      {{ card.title }}
    </v-card-title>
    <v-card-text>
      <div class="text-caption">{{ card.summary }}</div>
    </v-card-text>
    <v-card-actions v-if="!card.isSaved">
      <v-spacer></v-spacer>
      <v-btn size="small" variant="text" color="green" @click="emit('save', card.id)">{{ t('common.save') }}</v-btn>
      <v-btn size="small" variant="text" color="red" @click="emit('delete', card.id)">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

interface CardData {
  id: string;
  type: string;
  title: string;
  summary: string;
  isSaved?: boolean; // Added isSaved property
}

defineProps<{
  card: CardData;
  from: 'user' | 'ai'; // To determine card color based on sender
}>();

const emit = defineEmits<{
  (e: 'save', id: string): void;
  (e: 'delete', id: string): void;
}>();

const { t } = useI18n();

const getIconForType = (type: string): string => {
  switch (type) {
    case 'Member':
      return 'mdi-account';
    case 'Relationship':
      return 'mdi-link';
    case 'Event':
      return 'mdi-calendar-month';
    case 'Family':
      return 'mdi-home-group';
    default:
      return 'mdi-information';
  }
};
</script>

<style scoped>
/* Add any specific styles for the card here */
</style>
