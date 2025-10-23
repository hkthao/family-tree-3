<template>
  <v-row>
    <v-col cols="12">
      <AIBiographyInputPanel />
    </v-col>
    <v-col cols="12">
      <AIBiographyResultPanel />
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useAIBiographyStore } from '@/stores/aiBiography.store';
import { AIBiographyInputPanel, AIBiographyResultPanel } from '@/components/aiBiography';

const route = useRoute();
const aiBiographyStore = useAIBiographyStore();

const memberId = route.params.memberId as string;

watch(
  () => route.params.memberId,
  (newMemberId) => {
    if (newMemberId) {
      aiBiographyStore.memberId = newMemberId as string;
      aiBiographyStore.fetchMemberDetails(newMemberId as string);
    }
  },
  { immediate: true },
);

onMounted(() => {
  if (memberId) {
    aiBiographyStore.memberId = memberId;
    aiBiographyStore.fetchMemberDetails(memberId);
  }
});
</script>
