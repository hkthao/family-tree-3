<template>
  <v-avatar :size="size">
    <v-img v-if="displaySrc" :src="displaySrc"></v-img>
    <v-icon v-else :size="size">mdi-account-circle</v-icon>
  </v-avatar>
</template>

<script setup lang="ts">
import { computed } from 'vue';


const props = defineProps({
  src: { type: String, default: null },
  size: { type: Number, default: 128 },
});

// Regex to check if a string is a Base64 image data URL
const isBase64 = (str: string | null | undefined): boolean => {
  if (!str) return false;
  return str.startsWith('data:image/');
};

const processedSrc = computed(() => {
  if (isBase64(props.src)) {
    return props.src;
  }
  return props.src;
});

const displaySrc = computed(() => {
  return processedSrc.value || '/images/default-avatar.png';
});
</script>