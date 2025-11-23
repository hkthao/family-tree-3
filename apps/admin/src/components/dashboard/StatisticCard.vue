<template>
  <v-card elevation="2" class="s-card" hover>
    <v-card-text>
      <div class="d-flex justify-space-between align-center">
        <div>
          <p class="text-subtitle-1 font-weight-bold text-high-emphasis">{{ title }}</p>
          <h4 class="text-h5 font-weight-bold">{{ value }}</h4>
          <span :class="change.startsWith('+') ? 'text-success' : 'text-error'" class="text-subtitle-2">{{ change }}</span>
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

<style scoped>
.s-card {
  height: 105px;
}
</style>