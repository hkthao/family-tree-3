<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <AIBiographyInputPanel />
      </v-col>
      <v-col cols="12">
        <AIBiographyResultPanel
          v-model:biographyContent="biographyContent"
          @save="handleSaveBiography"
          @regenerate="handleRegenerateBiography"
        />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useAIBiographyStore } from '@/stores/ai-biography.store';
import { AIBiographyInputPanel, AIBiographyResultPanel } from '@/components/ai-biography';

const route = useRoute();
const aiBiographyStore = useAIBiographyStore();

const biographyContent = ref(''); // Managed in parent

const memberId = route.params.memberId as string;

const fetchAndSetMemberDetails = async (id: string) => {
  await aiBiographyStore.fetchMemberDetails(id);
  if (aiBiographyStore.currentMember?.biography) {
    biographyContent.value = aiBiographyStore.currentMember.biography;
  }
};

watch(
  () => route.params.memberId,
  (newMemberId) => {
    if (newMemberId) {
      aiBiographyStore.memberId = newMemberId as string;
      fetchAndSetMemberDetails(newMemberId as string);
    }
  },
  { immediate: true },
);

watch(
  () => aiBiographyStore.biographyResult,
  (newResult) => {
    if (newResult) {
      biographyContent.value = newResult.content;
    }
  },
);

onMounted(() => {
  if (memberId) {
    aiBiographyStore.memberId = memberId;
    fetchAndSetMemberDetails(memberId);
  }
});

const handleSaveBiography = (content: string) => {
  if (aiBiographyStore.memberId) {
    aiBiographyStore.saveBiography(aiBiographyStore.memberId, content);
  }
};

const handleRegenerateBiography = () => {
  aiBiographyStore.generateBiography();
};
</script>
