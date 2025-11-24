<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-btn icon @click="$router.back()" variant="text">
          <v-icon>mdi-arrow-left</v-icon>
        </v-btn>
        <span class="text-h6">{{ t('memory.edit.title') }}</span>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <MemoryEdit :memory-id="memoryId" @saved="handleSaved" />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import MemoryEdit from '@/components/memory/MemoryEdit.vue';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();

const memoryId = computed(() => route.params.memoryId as string);

const handleSaved = () => {
  // After saving, navigate back to the memory detail page
  if (memoryId.value) {
    router.push({ name: 'MemoryDetail', params: { memoryId: memoryId.value } });
  } else {
    router.back();
  }
};
</script>

<style scoped>
/* Add any specific styles */
</style>
