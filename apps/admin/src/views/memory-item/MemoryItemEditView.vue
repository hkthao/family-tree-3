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

    <MemoryMediaPickerDialog
      v-model="showMediaPicker"
      :family-id="familyId"
      @confirm="handleMediaConfirmed"
      :selected-media="initialSelectedMedia"
    />

  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import MemoryItemForm from '@/components/memory-item/MemoryItemForm.vue';
import { useMemoryItemEdit } from '@/composables';
import { type IMemoryItemFormInstance } from '@/components/memory-item/MemoryItemForm.vue';
import MemoryMediaPickerDialog from '@/components/memory-item/MemoryMediaPickerDialog.vue';
import { type MediaItem } from '@/types';


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

const showMediaPicker = ref(false);

const handleMediaConfirmed = (selectedItems: MediaItem[]) => {
  if (memoryItemFormRef.value) {
    (memoryItemFormRef.value as IMemoryItemFormInstance).addExistingMedia(selectedItems);
  }
  showMediaPicker.value = false;
};

const initialSelectedMedia = computed<MediaItem[]>(() => {
  if (!memoryItem.value || !memoryItem.value.memoryMedia) return [];
  return memoryItem.value.memoryMedia.map(media => ({
    id: media.id,
    url: media.url,
    type: media.type as unknown as string,
  }));
});

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