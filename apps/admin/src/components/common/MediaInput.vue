<template>
  <v-sheet class="d-flex flex-column border rounded pa-2">
    <div v-if="selectedMediaLocal.length > 0 && selectionMode === 'multiple'" class="d-flex flex-wrap mt-2">
      <v-chip v-for="media in selectedMediaLocal" :key="media.id" class="ma-1" :closable="!isDeleting"
        @click:close="removeMedia(media.id)">
        <span v-if="isDeleting" class="mr-2">
          <v-progress-circular indeterminate size="16" width="2"></v-progress-circular>
        </span>
        {{ media.fileName }}
      </v-chip>
    </div>

    <div v-else-if="selectedMediaLocal.length === 1 && selectionMode === 'single'" class="mt-2">
      <v-img :src="selectedMediaLocal[0].filePath" height="100px" contain></v-img>
      <v-chip class="ma-1" :closable="!isDeleting" @click:close="clearSelection">
        <span v-if="isDeleting" class="mr-2">
          <v-progress-circular indeterminate size="16" width="2"></v-progress-circular>
        </span>
        {{ selectedMediaLocal[0].fileName }}
      </v-chip>
    </div>
    <div class="mt-4 text-center">
      <v-btn :prepend-icon="'mdi-image-multiple'" @click="openMediaPicker" class="flex-grow-1 justify-start">
        <span class="text-truncate">{{ t('common.selectFile') }}</span>
      </v-btn>
    </div>
  </v-sheet>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n'; // Import useI18n
import { useMediaPickerDrawerStore } from '@/stores/mediaPickerDrawer.store';
import type { FamilyMedia } from '@/types';
import { MediaType } from '@/types/enums';
import { useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { useFamilyMediaDeleteMutation } from '@/composables/family-media/useFamilyMediaDeleteMutation';
import { useAuth } from '@/composables'; // NEW

type SelectionMode = 'single' | 'multiple';

const props = withDefaults(defineProps<{
  label?: string;
  familyId: string;
  selectionMode?: SelectionMode;
  modelValue: FamilyMedia[] | FamilyMedia | null; // v-model
  initialMediaType?: MediaType;

}>(), {
  label: 'Select Media',
  selectionMode: 'single',
  modelValue: null,

});

const emit = defineEmits(['update:modelValue']);

const mediaPickerStore = useMediaPickerDrawerStore();
const { t } = useI18n(); // Initialize t
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();
const { mutate: deleteMedia, isPending: isDeleting } = useFamilyMediaDeleteMutation();
const selectedMediaLocal = ref<FamilyMedia[]>([]);

const { state } = useAuth(); // NEW

const canUpload = computed(() => state.isAdmin.value || state.isFamilyManager.value(props.familyId)); // NEW
const canDelete = computed(() => state.isAdmin.value || state.isFamilyManager.value(props.familyId)); // NEW

onMounted(() => {
  // Initialize local state from modelValue
  if (props.modelValue) {
    selectedMediaLocal.value = Array.isArray(props.modelValue) ? props.modelValue : [props.modelValue];
  }
});

watch(() => props.modelValue, (newVal) => {
  if (newVal) {
    selectedMediaLocal.value = Array.isArray(newVal) ? newVal : [newVal];
  } else {
    selectedMediaLocal.value = [];
  }
}, { deep: true });

const openMediaPicker = async () => {
  try {
    const initialSelectionIds = selectedMediaLocal.value.map(media => media.id);
    const result = await mediaPickerStore.openDrawer({
      familyId: props.familyId,
      selectionMode: props.selectionMode,
      initialSelection: initialSelectionIds,
      initialMediaType: props.initialMediaType,
      allowUpload: canUpload.value,
      allowDelete: canDelete.value,
    });

    if (props.selectionMode === 'single') {
      if (result && !Array.isArray(result)) {
        selectedMediaLocal.value = [result];
      } else {
        selectedMediaLocal.value = [];
      }
    } else { // multiple
      if (result && Array.isArray(result)) {
        selectedMediaLocal.value = result;
      } else {
        selectedMediaLocal.value = [];
      }
    }
    emit('update:modelValue', props.selectionMode === 'single' ? (selectedMediaLocal.value[0] || null) : selectedMediaLocal.value);

  } catch (error) {
    console.error('Media selection cancelled or failed:', error);
  }
};

const clearSelection = () => {
  selectedMediaLocal.value = [];
  emit('update:modelValue', props.selectionMode === 'single' ? null : []);
};

const removeMedia = async (idToRemove: string) => {
  const mediaItemToRemove = selectedMediaLocal.value.find(media => media.id === idToRemove);
  if (!mediaItemToRemove) return;

  if (canDelete.value) {
    const confirmed = await showConfirmDialog({
      title: t('confirmDelete.title'),
      message: t('familyMedia.list.confirmDelete', { fileName: mediaItemToRemove.fileName }),
      confirmText: t('common.delete'),
      cancelText: t('common.cancel'),
      confirmColor: 'error',
    });

    if (!confirmed) {
      return; // Stop if not confirmed
    }

    deleteMedia(idToRemove, {
      onSuccess: () => {
        showSnackbar(t('familyMedia.messages.deleteSuccess'), 'success');
        selectedMediaLocal.value = selectedMediaLocal.value.filter(media => media.id !== idToRemove);
        emit('update:modelValue', props.selectionMode === 'single' ? (selectedMediaLocal.value[0] || null) : selectedMediaLocal.value);
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyMedia.messages.deleteError'), 'error');
      },
    });
  } else {
    // If deletion is not allowed, just remove from local selection
    selectedMediaLocal.value = selectedMediaLocal.value.filter(media => media.id !== idToRemove);
    emit('update:modelValue', props.selectionMode === 'single' ? (selectedMediaLocal.value[0] || null) : selectedMediaLocal.value);
  }
};
</script>

<style scoped>
/* Add any specific styles for MediaInput here */
</style>
