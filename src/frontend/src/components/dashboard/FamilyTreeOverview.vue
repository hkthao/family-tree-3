<template>
  <v-card :elevation="0">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-family-tree</v-icon>
      <span class="ml-2">{{ t('dashboard.familyTreeOverview.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="!familyId" class="text-center py-4">
        <v-icon size="64" color="grey lighten-1">mdi-information-outline</v-icon>
        <p class="mt-3 text-subtitle-1 text-grey">{{ t('dashboard.familyTreeOverview.selectFamilyPrompt') }}</p>
      </div>
      <TreeChart v-else :family-id="familyId" :read-only="true" />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import TreeChart from '@/components/family/TreeChart.vue'; // Corrected import path

const { t } = useI18n();

interface FamilyTreeOverviewProps {
  familyId: string | null;
}
defineProps<FamilyTreeOverviewProps>();
</script>
