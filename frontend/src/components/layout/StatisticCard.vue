<template>
  <v-card :color="color" class="text-white" elevation="2">
    <v-card-text class="d-flex align-center justify-space-between">
      <div>
        <div class="text-h6">{{ value }}</div>
        <div class="text-subtitle-2">{{ title }}</div>
      </div>
      <v-icon :icon="icon" size="40"></v-icon>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
interface Props {
  title: string;
  value: string;
  icon: string;
  color: string;
}

defineProps<Props>();
</script>