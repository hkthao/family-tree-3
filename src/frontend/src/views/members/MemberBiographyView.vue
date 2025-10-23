<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h5 mb-4">{{ t('aiBiography.generator.title') }}</h1>
        <p class="text-subtitle-1 text-medium-emphasis mb-6">{{ t('aiBiography.generator.description') }}</p>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <AIBiographyInputPanel />
      </v-col>
      <v-col cols="12">
        <AIBiographyResultPanel />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useAIBiographyStore } from '@/stores/aiBiography.store';
import { AIBiographyInputPanel, AIBiographyResultPanel } from '@/components/aiBiography';

const { t } = useI18n();
const route = useRoute();
const aiBiographyStore = useAIBiographyStore();

const memberId = route.params.memberId as string;

watch(
  () => route.params.memberId,
  (newMemberId) => {
    if (newMemberId) {
      aiBiographyStore.memberId = newMemberId as string;
      aiBiographyStore.fetchMemberDetails(newMemberId as string);
      aiBiographyStore.fetchAIProviders();
    }
  },
  { immediate: true },
);

onMounted(() => {
  if (memberId) {
    aiBiographyStore.memberId = memberId;
    aiBiographyStore.fetchMemberDetails(memberId);
    aiBiographyStore.fetchAIProviders();
  }
});
</script>
