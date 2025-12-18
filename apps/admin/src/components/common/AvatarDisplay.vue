<template>
  <v-avatar :size="size">
    <v-img v-if="displaySrc" :src="displaySrc"></v-img>
    <v-icon v-else :size="size">mdi-account-circle</v-icon>
  </v-avatar>
</template>

<script setup lang="ts">
import { useAuthenticatedImage } from '@/composables/ui/useAuthenticatedImage';

const props = defineProps({
  src: { type: String, default: null },
  size: { type: Number, default: 128 },
});

const { displaySrc } = useAuthenticatedImage(props.src);
</script>