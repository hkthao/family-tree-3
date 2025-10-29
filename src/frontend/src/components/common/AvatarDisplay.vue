<template>
  <v-avatar :size="size">
    <v-img v-if="displaySrc" :src="displaySrc"></v-img>
    <v-icon v-else :size="size">mdi-account-circle</v-icon>
  </v-avatar>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import axios from 'axios';
import { useAuthStore } from '@/stores';

const props = defineProps({
  src: { type: String, default: null },
  size: { type: Number, default: 128 },
});

const authStore = useAuthStore();
const displaySrc = ref<string | null>(null);

const fetchAuthenticatedImage = async (imageUrl: string) => {
  if (!imageUrl) {
    displaySrc.value = null;
    return;
  }

  // Check if it's a data URL or a non-authenticated URL
  if (imageUrl.startsWith('data:') || !imageUrl.includes('/api/upload/preview/')) {
    displaySrc.value = imageUrl;
    return;
  }

  try {
    const token = authStore.getAccessToken; // Use the getter
    const response = await axios.get(imageUrl, {
      responseType: 'blob',
      headers: {
        Authorization: token ? `Bearer ${token}` : '',
      },
    });

    if (response.data) {
      displaySrc.value = URL.createObjectURL(response.data);
    } else {
      displaySrc.value = null;
    }
  } catch (error) {
    console.error('Failed to fetch authenticated image:', error);
    displaySrc.value = null; // Fallback to default avatar
  }
};

watch(
  () => props.src,
  (newSrc) => {
    fetchAuthenticatedImage(newSrc || '');
  },
  { immediate: true },
);

// Clean up the object URL when the component is unmounted
onMounted(() => {
  // No specific cleanup needed here as URL.revokeObjectURL is handled by the browser when the object URL is no longer referenced
});
</script>