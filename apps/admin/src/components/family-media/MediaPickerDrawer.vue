// apps/admin/src/components/media/MediaPickerDrawer.vue
<template>
  <v-navigation-drawer
    v-model="mediaPickerStore.drawer"
    location="right"
    width="500"
    temporary
    class="media-picker-drawer"
  >
    <v-card-title class="d-flex align-center justify-space-between bg-primary text-white">
      <span>Chọn Media</span>
      <v-btn icon flat @click="mediaPickerStore.closeDrawer()">
        <v-icon>mdi-close</v-icon>
      </v-btn>
    </v-card-title>
    <v-card-text class="pa-0">
      <MediaPickerContent
        v-if="mediaPickerStore.familyId"
        :family-id="mediaPickerStore.familyId"
        v-model:selectedMedia="selectedMediaLocal"
        @update:selectedMedia="handleSelectionUpdate"
      />
    </v-card-text>
    <v-card-actions class="d-flex justify-end">
      <v-btn variant="text" @click="mediaPickerStore.closeDrawer()">Hủy</v-btn>
      <v-btn variant="elevated" color="primary" @click="confirmSelection()">Chọn</v-btn>
    </v-card-actions>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useMediaPickerDrawerStore } from '@/stores/mediaPickerDrawer.store';
import MediaPickerContent from './MediaPickerContent.vue';
import type { FamilyMedia } from '@/types'; // Assuming this type includes id, url, etc.

const mediaPickerStore = useMediaPickerDrawerStore();
const selectedMediaLocal = ref<string[]>([]); // Array of IDs

// Initialize selectedMediaLocal when drawer opens or initialSelection changes
watch(
  () => mediaPickerStore.drawer,
  (newVal) => {
    if (newVal) {
      if (mediaPickerStore.initialSelection) {
        // Ensure initialSelection is always an array for local management
        selectedMediaLocal.value = Array.isArray(mediaPickerStore.initialSelection)
          ? mediaPickerStore.initialSelection
          : [mediaPickerStore.initialSelection];
      } else {
        selectedMediaLocal.value = [];
      }
    }
  },
  { immediate: true }
);

const handleSelectionUpdate = (newSelection: string[]) => {
  selectedMediaLocal.value = newSelection;
};

const confirmSelection = () => {
  // Logic to get full media objects based on selected IDs
  // For simplicity, we'll just return the IDs for now.
  // In a real scenario, you might want to fetch the full FamilyMedia objects
  // or pass them from MediaPickerContent up to here.

  if (mediaPickerStore.selectionMode === 'single') {
    mediaPickerStore.confirmSelection(selectedMediaLocal.value.length > 0 ? selectedMediaLocal.value[0] as unknown as FamilyMedia : null);
  } else {
    // This will require a way to get the actual FamilyMedia objects from their IDs
    // For now, we'll pass the IDs. A more robust solution would involve fetching these.
    // Or MediaPickerContent could emit the full objects.
    mediaPickerStore.confirmSelection(selectedMediaLocal.value as unknown as FamilyMedia[]);
  }
};
</script>

<style scoped>
.media-picker-drawer {
  z-index: 1000; /* Ensure it's above other content */
}
</style>
