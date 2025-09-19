<template>
  <v-card class="rounded-xl" elevation="2" style="transition: all 0.3s ease-in-out;" hover>
    <v-card-text>
      <div class="d-flex justify-space-between align-center">
        <div>
          <p class="text-caption">{{ title }}</p>
          <h4 class="text-h5 font-weight-bold">{{ value }}</h4>
          <span :class="change.startsWith('+') ? 'text-success' : 'text-error'" class="text-caption">{{ change }}</span>
        </div>
        <v-avatar :color="color" rounded="lg" size="42">
          <v-icon :icon="icon" color="white"></v-icon>
        </v-avatar>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
defineProps({
  title: String,
  value: String,
  change: { type: String, default: '' },
  icon: String,
  color: String,
});
</script>
