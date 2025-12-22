<template>
  <v-card :elevation="0" data-testid="memory-item-edit-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('memoryItem.form.editTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <MemoryItemForm
        ref="memoryItemFormRef"
        v-if="memoryItem"
        :initial-memory-item-data="memoryItem"
        :family-id="props.familyId"
        :read-only="false"
      />
      <v-progress-circular v-else indeterminate color="primary"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn
        color="grey"
        data-testid="button-cancel"
        @click="closeForm"
        :disabled="isLoading || isUpdatingMemoryItem || isUploadingMedia"
      >{{ t('common.cancel') }}</v-btn>
      <v-btn color="secondary" data-testid="button-select-media" @click="showMediaPicker = true"
        :disabled="isLoading || isUpdatingMemoryItem || isUploadingMedia">
        {{ t('memoryItem.form.selectMedia') }}
      </v-btn>
      <v-btn
        color="primary"
        data-testid="button-save"
        @click="handleUpdateItem"
        :loading="isUpdatingMemoryItem || isUploadingMedia"
        :disabled="isLoading || isUpdatingMemoryItem || isUploadingMedia"
      >{{ t('common.save') }}</v-btn>
    </v-card-actions>

    <v-dialog v-model="showMediaPicker" max-width="800">
      <v-card>
        <v-card-title>{{ t('memoryItem.form.selectMedia') }}</v-card-title>
        <v-card-text>
          <MediaPicker :family-id="familyId" selection-mode="multiple" @selected="handleMediaSelected" />
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn @click="showMediaPicker = false">{{ t('common.cancel') }}</v-btn>
          <v-btn color="primary" @click="confirmMediaSelection">{{ t('common.select') }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import MemoryItemForm from '@/components/memory-item/MemoryItemForm.vue';
import { useMemoryItemEdit } from '@/composables';
import { type IMemoryItemFormInstance } from '@/components/memory-item/MemoryItemForm.vue';
import MediaPicker from '@/components/family-media/MediaPicker.vue'; // Import MediaPicker
import { type MediaItem } from '@/types'; // Import MediaItem

const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
  memoryItemId: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close', 'saved']);

const memoryItemFormRef: Ref<IMemoryItemFormInstance | null> = ref(null);

const { t } = useI18n();

const showMediaPicker = ref(false); // Reactive variable to control MediaPicker dialog visibility
const selectedMediaFromPicker = ref<MediaItem[]>([]); // Reactive variable to store selected media

// Handle media selected from the MediaPicker
const handleMediaSelected = (selectedItems: MediaItem[] | MediaItem | null) => {
  if (Array.isArray(selectedItems)) {
    selectedMediaFromPicker.value = selectedItems;
  } else if (selectedItems === null) {
    selectedMediaFromPicker.value = [];
  }
};

// Confirm media selection and add to the form
const confirmMediaSelection = () => {
  if (memoryItemFormRef.value) {
    (memoryItemFormRef.value as IMemoryItemFormInstance).addExistingMedia(selectedMediaFromPicker.value);
  }
  showMediaPicker.value = false;
};

const {
  state: { memoryItem, isLoading, isUpdatingMemoryItem, isUploadingMedia },
  actions: { handleUpdateItem, closeForm },
} = useMemoryItemEdit({
  familyId: props.familyId,
  memoryItemId: props.memoryItemId,
  onSaveSuccess: () => {
    emit('saved');
  },
  onCancel: () => {
    emit('close');
  },
  formRef: memoryItemFormRef,
});
</script>

<style scoped></style>