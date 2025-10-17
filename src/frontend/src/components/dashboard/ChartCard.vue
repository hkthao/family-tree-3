<template>
  <v-card class="rounded-xl h-100" elevation="2" style="transition: all 0.3s ease-in-out;" hover>
    <v-card-item>
      <v-card-title class="text-h6">{{ title }}</v-card-title>
    </v-card-item>
    <v-card-text>
      <slot></slot>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
defineProps({
  title: String,
});
</script>
