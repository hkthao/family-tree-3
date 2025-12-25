import { defineStore } from 'pinia';
import { ref } from 'vue';

interface Coordinates {
  latitude: number;
  longitude: number;
}

export const useMapLocationDrawerStore = defineStore('mapLocationDrawer', () => {
  const drawer = ref(false);
  const selectedLocation = ref<{ coordinates: Coordinates | null; location: string }>({ coordinates: null, location: '' });
  let resolvePromise: ((value: { coordinates: Coordinates | null; location: string }) => void) | null = null;
  let rejectPromise: ((reason?: any) => void) | null = null;

  function openDrawer(): Promise<{ coordinates: Coordinates | null; location: string }> {
    drawer.value = true;
    return new Promise((resolve, reject) => {
      resolvePromise = resolve;
      rejectPromise = reject;
    });
  }

  function closeDrawer() {
    drawer.value = false;
    if (rejectPromise) {
      rejectPromise(new Error('Map location selection cancelled'));
      resolvePromise = null;
      rejectPromise = null;
    }
  }

  function confirmSelection(coordinates: Coordinates, location: string) {
    selectedLocation.value = { coordinates, location };
    drawer.value = false;
    if (resolvePromise) {
      resolvePromise(selectedLocation.value);
      resolvePromise = null;
      rejectPromise = null;
    }
  }

  return {
    drawer,
    selectedLocation,
    openDrawer,
    closeDrawer,
    confirmSelection,
  };
});
