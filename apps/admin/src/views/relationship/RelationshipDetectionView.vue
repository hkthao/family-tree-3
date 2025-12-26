<template>
  <v-container fluid>
    <v-card>
      <v-card-title class="text-h5 text-center">{{ t('relationshipDetection.title') }}</v-card-title>
      <v-card-text>
        <RelationshipDetector :initial-family-id="familyId" />
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import RelationshipDetector from '@/components/relationship/RelationshipDetector.vue';

const { familyId } = defineProps<{
  familyId?: string; // Optional familyId prop
}>();

const { t } = useI18n();
</script>

<style scoped>
/* Add any specific styles here if needed */
</style>
