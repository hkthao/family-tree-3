// apps/admin/src/stores/mediaPickerDrawer.store.ts
import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { FamilyMedia } from '@/types';
import { MediaType } from '@/types/enums';

type SelectionMode = 'single' | 'multiple';

interface MediaPickerOptions {
  familyId: string;
  selectionMode?: SelectionMode;
  initialSelection?: string[] | string;
  initialMediaType?: MediaType;
}

export const useMediaPickerDrawerStore = defineStore('mediaPickerDrawer', () => {
  const drawer = ref(false);
  const familyId = ref<string | null>(null);
  const selectionMode = ref<SelectionMode>('single');
  const initialSelection = ref<string[] | string>([]);
  const initialMediaType = ref<MediaType | null>(null);

  let resolvePromise: ((value: FamilyMedia[] | FamilyMedia | null) => void) | null = null;
  let rejectPromise: ((reason?: any) => void) | null = null;

  function openDrawer(options: MediaPickerOptions): Promise<FamilyMedia[] | FamilyMedia | null> {
    familyId.value = options.familyId;
    selectionMode.value = options.selectionMode || 'single';
    initialSelection.value = options.initialSelection || (options.selectionMode === 'single' ? '' : []);
    initialMediaType.value = options.initialMediaType || null;
    drawer.value = true;

    return new Promise((resolve, reject) => {
      resolvePromise = resolve;
      rejectPromise = reject;
    });
  }

  function closeDrawer() {
    drawer.value = false;
    familyId.value = null;
    initialSelection.value = [];
    initialMediaType.value = null;
    if (rejectPromise) {
      rejectPromise(new Error('Media selection cancelled'));
      resolvePromise = null;
      rejectPromise = null;
    }
  }

  function confirmSelection(selectedMedia: FamilyMedia[] | FamilyMedia | null) {
    drawer.value = false;
    familyId.value = null;
    initialSelection.value = [];
    initialMediaType.value = null;
    if (resolvePromise) {
      resolvePromise(selectedMedia);
      resolvePromise = null;
      rejectPromise = null;
    }
  }

  return {
    drawer,
    familyId,
    selectionMode,
    initialSelection,
    initialMediaType,
    openDrawer,
    closeDrawer,
    confirmSelection,
  };
});
