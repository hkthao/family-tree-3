<template>
  <v-list>
    <v-sheet v-for="(item, index) in voiceGenerations" :key="item.id" class="mb-2 pa-2" border rounded elevation="1">
      <div class="d-flex align-center">
        <v-icon class="mr-2">mdi-volume-high</v-icon>
        <div class="w-100">
          <div class="text-subtitle-1">{{ item.text }}</div>
          <audio controls :src="item.audioUrl" class="w-100"></audio>
        </div>
        <v-btn v-if="allowDelete" icon variant="text" color="error" size="small" @click="emit('delete', item.id)">
          <v-icon>mdi-delete</v-icon>
        </v-btn>
      </div>
      <v-divider v-if="index < voiceGenerations.length - 1" class="my-2"></v-divider>
    </v-sheet>
  </v-list>
</template>

<script setup lang="ts">
import type { VoiceGenerationDto } from '@/types';
defineProps<{
  voiceGenerations: VoiceGenerationDto[];
  allowDelete?: boolean; // New prop
}>();

const emit = defineEmits(['delete']); // New emit event
</script>

<style scoped>
/* No specific styles needed for just the list */
</style>