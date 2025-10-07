<template>
  <v-card class="pa-4" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-family-tree</v-icon>
      <span class="ml-2">Tổng quan cây gia phả</span>
    </v-card-title>
    <v-card-text>
      <div v-if="!familyId" class="text-center py-4">
        <v-icon size="64" color="grey lighten-1">mdi-information-outline</v-icon>
        <p class="mt-3 text-subtitle-1 text-grey">Vui lòng chọn một gia đình để xem cây gia phả.</p>
      </div>
      <TreeChart v-else :family-id="familyId" />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import TreeChart from '@/components/family/TreeChart.vue'; // Corrected import path

interface FamilyTreeOverviewProps {
  familyId: string | null;
}

const props = defineProps<FamilyTreeOverviewProps>();
</script>

<style scoped>
/* Add any specific styles for FamilyTreeOverview here */
</style>