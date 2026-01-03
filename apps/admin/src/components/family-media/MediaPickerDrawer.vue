<template>
  <v-navigation-drawer v-model="mediaPickerStore.drawer" location="right" width="650" temporary
    class="media-picker-drawer">
    <v-card-title class="d-flex align-center justify-space-between">
      <v-btn icon variant="text" @click="mediaPickerStore.closeDrawer()">
        <v-icon>mdi-close</v-icon>
      </v-btn>
      {{ t('common.selectMedia') }}
      <v-spacer />
    </v-card-title>
    <v-card-text>
      <VFileUpload v-if="mediaPickerStore.allowUpload && canUploadMedia" v-model="fileToUpload" :label="t('familyMedia.form.fileLabel')"
        prepend-icon="mdi-paperclip" :multiple="true" show-size :clearable="true"
        @update:modelValue="handleFilesUpdate"></VFileUpload>
      <v-btn v-if="mediaPickerStore.allowUpload && canUploadMedia && fileToUpload.length > 0" color="primary" class="mt-2"
        :loading="isUploading" :disabled="isUploading" @click="uploadFiles">
        {{ t('familyMedia.upload.uploadButton') }}
      </v-btn>

      <MediaPickerContent v-if="mediaPickerStore.familyId" :family-id="mediaPickerStore.familyId"
        :selection-mode="mediaPickerStore.selectionMode" v-model:selectedMedia="selectedMediaIds"
        @update:selectedMedia="handleSelectionUpdate" :allow-delete="mediaPickerStore.allowDelete && canDeleteMedia"
        :initial-media-type="mediaPickerStore.initialMediaType" />
    </v-card-text>
    <v-card-actions class="d-flex justify-end">
      <v-btn variant="text" @click="mediaPickerStore.closeDrawer()">{{ t('common.cancel') }}</v-btn>
      <v-btn variant="elevated" color="primary" @click="confirmSelection()">{{ t('common.select') }}</v-btn>
    </v-card-actions>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQueryClient } from '@tanstack/vue-query';
// Removed VFileInput as VFileUpload is used
import { VFileUpload } from 'vuetify/labs/VFileUpload'; // Import VFileUpload
import { useGlobalSnackbar } from '@/composables';
import { useMediaPickerDrawerStore } from '@/stores/mediaPickerDrawer.store';
import MediaPickerContent from './MediaPickerContent.vue';
import type { FamilyMedia } from '@/types';
import { useFamilyMediaUploadMutation } from '@/composables/family-media/useFamilyMediaUploadMutation'; // Import new mutation
import { useAuth } from '@/composables';

const { t } = useI18n();
const queryClient = useQueryClient(); // For invalidating queries
const { showSnackbar } = useGlobalSnackbar();
const mediaPickerStore = useMediaPickerDrawerStore();
const { state: authState } = useAuth(); // Renamed to authState to avoid conflict

const canUploadMedia = computed(() => {
  if (!mediaPickerStore.familyId) return false;
  return authState.isAdmin.value || authState.isFamilyManager.value(mediaPickerStore.familyId);
});

const canDeleteMedia = computed(() => {
  if (!mediaPickerStore.familyId) return false;
  return authState.isAdmin.value || authState.isFamilyManager.value(mediaPickerStore.familyId);
});

const fileToUpload = ref<File[]>([]); // Changed to array of File
const { mutateAsync: uploadMedia, isPending: isUploading } = useFamilyMediaUploadMutation(); // Use mutateAsync for awaiting multiple uploads

const selectedMediaLocal = ref<FamilyMedia[]>([]);

const selectedMediaIds = computed(() => selectedMediaLocal.value.map(media => media.id));

watch(
  () => mediaPickerStore.drawer,
  (newVal) => {
    if (newVal) {
      selectedMediaLocal.value = [];
      fileToUpload.value = []; // Clear file input on drawer open
    }
  },
  { immediate: true }
);

const handleSelectionUpdate = (newSelection: FamilyMedia[]) => {
  selectedMediaLocal.value = newSelection;
};

const handleFilesUpdate = (files: File[]) => {
  fileToUpload.value = files; // Directly assign the array of files
};

const uploadFiles = async () => {
  if (fileToUpload.value.length === 0 || !mediaPickerStore.familyId) {
    showSnackbar(t('familyMedia.errors.noFileSelected'), 'warning');
    return;
  }

  const currentFamilyId = mediaPickerStore.familyId as string; // Assert as string after null check

  const uploadPromises = fileToUpload.value.map(file =>
    uploadMedia({ familyId: currentFamilyId, file: file, description: '' })
  );

  try {
    await Promise.all(uploadPromises);
    showSnackbar(t('familyMedia.messages.uploadSuccess'), 'success');
    fileToUpload.value = []; // Clear input after upload
    queryClient.invalidateQueries({ queryKey: ['familyMedia'] }); // Refresh media list
  } catch (error: any) {
    showSnackbar(error.message || t('familyMedia.messages.saveError'), 'error');
  }
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
  z-index: 1000;
  /* Ensure it's above other content */
}
</style>
