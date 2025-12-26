import { defineStore } from 'pinia';
import { ref } from 'vue';

interface Coordinates {
  latitude: number;
  longitude: number;
}

interface LocationData {
  latitude: number;
  longitude: number;
  address?: string;
}

export const useMapLocationDrawerStore = defineStore('mapLocationDrawer', () => {
  const drawer = ref(false);
  const selectedLocation = ref<{ coordinates: Coordinates | null; location: string }>({ coordinates: null, location: '' });
  const initialLocation = ref<LocationData | undefined>(undefined); // New ref for initial location

  let resolvePromise: ((value: { coordinates: Coordinates | null; location: string }) => void) | null = null;
  let rejectPromise: ((reason?: any) => void) | null = null;

  function openDrawer(location?: LocationData): Promise<{ coordinates: Coordinates | null; location: string }> {
    initialLocation.value = location; // Store the initial location
    drawer.value = true;
    return new Promise((resolve, reject) => {
      resolvePromise = resolve;
      rejectPromise = reject;
    });
  }

  function closeDrawer() {
    drawer.value = false;
    initialLocation.value = undefined; // Clear initial location on close
    if (rejectPromise) {
      rejectPromise(new Error('Map location selection cancelled'));
      resolvePromise = null;
      rejectPromise = null;
    }
  }

  function confirmSelection(coordinates: Coordinates, location: string) {
    selectedLocation.value = { coordinates, location };
    drawer.value = false;
    initialLocation.value = undefined; // Clear initial location on confirm
    if (resolvePromise) {
      resolvePromise(selectedLocation.value);
      resolvePromise = null;
      rejectPromise = null;
    }
  }

  return {
    drawer,
    selectedLocation,
    initialLocation, // Expose initialLocation
    openDrawer,
    closeDrawer,
    confirmSelection,
  };
});
