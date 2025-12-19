import { ref, watch } from 'vue';
import axios from 'axios';
import { useAuthStore } from '@/stores';

export function useAuthenticatedImage(initialSrc: string | null) {
  const authStore = useAuthStore();
  const displaySrc = ref<string | null>(initialSrc);

  const fetchAuthenticatedImage = async (imageUrl: string | null) => {
    if (!imageUrl) {
      displaySrc.value = null;
      return;
    }

    // If it's a data URL, display it directly.
    if (imageUrl.startsWith('data:')) {
      displaySrc.value = imageUrl;
      return;
    }

    // For any other URL, attempt to fetch with authentication header.

    try {
      const token = authStore.getAccessToken;
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
      displaySrc.value = null;
    }
  };

  watch(
    () => initialSrc,
    (newSrc) => {
      fetchAuthenticatedImage(newSrc);
    },
    { immediate: true },
  );

  return {
    displaySrc,
  };
}
