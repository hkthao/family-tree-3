import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { FamilyLocation } from '@/types';

export const useLocationDrawerStore = defineStore('locationDrawer', () => {
  const drawer = ref(false);
  const selectedFamilyLocation = ref<FamilyLocation | null>(null);
  const initialFamilyId = ref<string | null>(null); // To filter locations by family

  let resolvePromise: ((value: FamilyLocation) => void) | null = null;
  let rejectPromise: ((reason?: any) => void) | null = null;

  function openDrawer(familyId?: string | null): Promise<FamilyLocation> {
    initialFamilyId.value = familyId || null;
    drawer.value = true;
    return new Promise((resolve, reject) => {
      resolvePromise = resolve;
      rejectPromise = reject;
    });
  }

  function closeDrawer() {
    drawer.value = false;
    initialFamilyId.value = null; // Clear familyId on close
    if (rejectPromise) {
      rejectPromise(new Error('Family Location selection cancelled'));
      resolvePromise = null;
      rejectPromise = null;
    }
  }

  function confirmSelection(location: FamilyLocation) {
    selectedFamilyLocation.value = location;
    drawer.value = false;
    initialFamilyId.value = null; // Clear familyId on confirm
    if (resolvePromise) {
      resolvePromise(selectedFamilyLocation.value);
      resolvePromise = null;
      rejectPromise = null;
    }
  }

  return {
    drawer,
    selectedFamilyLocation,
    initialFamilyId,
    openDrawer,
    closeDrawer,
    confirmSelection,
  };
});
