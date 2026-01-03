<template>
  <v-navigation-drawer
    v-model="mediaPickerStore.drawer"
    location="right"
    width="650"
    temporary
    class="media-picker-drawer"
  >
    <v-card-title class="d-flex align-center justify-space-between">
     <v-btn icon variant="text" @click="mediaPickerStore.closeDrawer()">
        <v-icon>mdi-close</v-icon>
      </v-btn>
      {{ t('common.selectMedia') }}
      <v-spacer/>
    </v-card-title>
    <v-card-text class="pa-0">
      <MediaPickerContent
        v-if="mediaPickerStore.familyId"
        :family-id="mediaPickerStore.familyId"
        :selection-mode="mediaPickerStore.selectionMode"
        v-model:selectedMedia="selectedMediaIds"
        @update:selectedMedia="handleSelectionUpdate"
      />
    </v-card-text>
    <v-card-actions class="d-flex justify-end">
      <v-btn variant="text" @click="mediaPickerStore.closeDrawer()">{{ t('common.cancel') }}</v-btn>
      <v-btn variant="elevated" color="primary" @click="confirmSelection()">{{ t('common.select') }}</v-btn>
    </v-card-actions>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n'; // Import useI18n
import { useMediaPickerDrawerStore } from '@/stores/mediaPickerDrawer.store';
import MediaPickerContent from './MediaPickerContent.vue';
import type { FamilyMedia } from '@/types';

const { t } = useI18n(); // Initialize t
const mediaPickerStore = useMediaPickerDrawerStore();
const selectedMediaLocal = ref<FamilyMedia[]>([]); // Array of full FamilyMedia objects

// Computed property to convert FamilyMedia[] to string[] for MediaPickerContent prop
const selectedMediaIds = computed(() => selectedMediaLocal.value.map(media => media.id));

// Initialize selectedMediaLocal when drawer opens or initialSelection changes
watch(
  () => mediaPickerStore.drawer,
  (newVal) => {
    if (newVal) {
      // Need to convert initialSelection (string[] or string) to FamilyMedia[]
      // This is a potential weakness. Ideally, initialSelection should be FamilyMedia[]
      // For now, we assume initialSelection only contains IDs if selectionMode is multiple,
      // or a single ID if selectionMode is single.
      // We won't try to fetch full FamilyMedia objects for initialSelection here,
      // as MediaPickerContent will determine selected states based on IDs.
      // So, selectedMediaLocal for the drawer will start empty until selection happens in content.
      selectedMediaLocal.value = [];
    }
  },
  { immediate: true }
);

const handleSelectionUpdate = (newSelection: FamilyMedia[]) => {
  selectedMediaLocal.value = newSelection;
};

const confirmSelection = () => {
  if (mediaPickerStore.selectionMode === 'single') {
    mediaPickerStore.confirmSelection(selectedMediaLocal.value.length > 0 ? selectedMediaLocal.value[0] : null);
  } else {
    mediaPickerStore.confirmSelection(selectedMediaLocal.value);
  }
};
</script>

<style scoped>
.media-picker-drawer {
  z-index: 1000; /* Ensure it's above other content */
}
</style>
