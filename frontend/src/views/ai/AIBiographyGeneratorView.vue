<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12" md="12">
        <AIBiographyInputPanel />
      </v-col>
      <v-col cols="12" md="12">
        <AIBiographyResultPanel />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useRoute } from 'vue-router';

import AIBiographyInputPanel from '@/components/aiBiography/AIBiographyInputPanel.vue';
import AIBiographyResultPanel from '@/components/aiBiography/AIBiographyResultPanel.vue';
import { useAIBiographyStore } from '@/stores/aiBiography.store';


const route = useRoute();
const aiBiographyStore = useAIBiographyStore();

onMounted(() => {
  if (route.params.memberId) {
    aiBiographyStore.memberId = route.params.memberId as string;
    aiBiographyStore.fetchLastAIBiography(aiBiographyStore.memberId);
  }
});
</script>
