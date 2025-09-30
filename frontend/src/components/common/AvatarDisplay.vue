<template>
  <v-avatar :size="size">
    <v-img v-if="src" :src="src"></v-img>
    <v-icon v-else :size="size">mdi-account-circle</v-icon>
  </v-avatar>
</template>

<script setup lang="ts">
import { defineProps } from 'vue';

defineProps({
  src: { type: String, default: null },
  size: { type: Number, default: 128 },
});
</script>
