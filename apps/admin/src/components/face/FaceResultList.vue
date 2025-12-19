<template>
  <v-card class="face-result-list-card" elevation="2">
    <v-list v-if="results.length > 0">
      <v-list-item
        v-for="result in results"
        :key="result.member.id"
        @click="goToMemberProfile(result.member.id)"
        link
      >
        <template v-slot:prepend>
          <v-avatar size="40" rounded="sm">
            <v-img :src="result.member.avatarUrl" :alt="result.member.fullName"></v-img>
          </v-avatar>
        </template>
        <v-list-item-title>{{ result.member.fullName }}</v-list-item-title>
        <v-list-item-subtitle>{{ t('face.resultList.confidence', { confidence: (result.confidence * 100).toFixed(0) }) }}</v-list-item-subtitle>
        <template v-slot:append>
          <v-icon>mdi-chevron-right</v-icon>
        </template>
      </v-list-item>
    </v-list>
    <v-card-text v-else>
      <v-alert type="info" variant="tonal">{{ t('face.resultList.noResults') }}</v-alert>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import type { SearchResult } from '@/types';
import { useFaceResultList } from '@/composables';

const props = defineProps({
  results: { type: Array as () => SearchResult[], default: () => [] },
});

const {
  t,
  goToMemberProfile,
} = useFaceResultList(props);
</script>

<style scoped>
.face-result-list-card {
  max-width: 600px;
  margin: auto;
}
</style>
