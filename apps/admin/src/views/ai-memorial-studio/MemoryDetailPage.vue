<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-btn icon @click="$router.back()" variant="text">
          <v-icon>mdi-arrow-left</v-icon>
        </v-btn>
        <span class="text-h6">{{ t('memory.detail.titleDefault') }}</span>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <MemoryDetail :memory-id="memoryId" @deleted="handleDeleted" />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import MemoryDetail from '@/components/memory/MemoryDetail.vue';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();

const memoryId = computed(() => route.params.memoryId as string);

const handleDeleted = () => {
  // If memory is deleted, navigate back to the member memories list
  if (route.params.memberId) {
    router.push({ name: 'MemberMemories', params: { memberId: route.params.memberId } });
  } else {
    router.back(); // Fallback if memberId is not in route params
  }
};
</script>

<style scoped>
/* Add any specific styles */
</style>
