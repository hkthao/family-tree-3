<template>
  <v-card :elevation="0" data-testid="memory-item-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('memoryItem.form.addTitle')
      }}</span>
    </v-card-title>
    <v-card-text>
      <MemoryItemForm ref="memoryItemFormRef" :family-id="familyId" @cancel="closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm"
        :disabled="isAddingMemoryItem || isUploadingMedia">{{
          t('common.cancel')
        }}</v-btn>
      <v-btn color="secondary" data-testid="button-select-media" @click="showMediaPicker = true"
        :disabled="isAddingMemoryItem || isUploadingMedia">
        {{ t('memoryItem.form.selectMedia') }}
      </v-btn>
      <v-btn color="primary" data-testid="button-save" @click="handleAddItem"
        :loading="isAddingMemoryItem || isUploadingMedia" :disabled="isAddingMemoryItem || isUploadingMedia">{{
          t('common.save') }}</v-btn>
    </v-card-actions>

    <MemoryMediaPickerDialog
      v-model="showMediaPicker"
      :family-id="familyId"
      @confirm="handleMediaConfirmed"
    />

  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import MemoryItemForm from '@/components/memory-item/MemoryItemForm.vue';
import { useMemoryItemAdd } from '@/composables';
import { type MediaItem } from '@/types';
import { type IMemoryItemFormInstance } from '@/components/memory-item/MemoryItemForm.vue';
import MemoryMediaPickerDialog from '@/components/memory-item/MemoryMediaPickerDialog.vue'; // New import

const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
});

const memoryItemFormRef: Ref<IMemoryItemFormInstance | null> = ref(null);
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();

const showMediaPicker = ref(false);

const handleMediaConfirmed = (selectedItems: MediaItem[]) => {
  if (memoryItemFormRef.value) {
    (memoryItemFormRef.value as IMemoryItemFormInstance).addExistingMedia(selectedItems);
  }
  showMediaPicker.value = false;
};

const {
  state: { isAddingMemoryItem, isUploadingMedia },
  actions: { handleAddItem, closeForm },
} = useMemoryItemAdd({
  familyId: props.familyId,
  onSaveSuccess: () => {
    emit('close');
    emit('saved');
  },
  onCancel: () => {
    emit('close');
  },
  formRef: memoryItemFormRef,
});
</script>

<style scoped>
.v-card-text-scrollable {
  max-height: 70vh; /* Adjust as needed */
  overflow-y: auto;
}
</style>