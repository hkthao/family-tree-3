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
        :disabled="isLoading || isUpdatingMemoryItem"
      >{{ t('common.cancel') }}</v-btn>
      <v-btn
        color="primary"
        data-testid="button-save"
        @click="handleUpdateItem"
        :loading="isUpdatingMemoryItem"
        :disabled="isLoading || isUpdatingMemoryItem"
      >{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import MemoryItemForm, { type MemoryItemFormExpose } from '@/components/memory-item/MemoryItemForm.vue';

import { useMemoryItemEdit } from '@/composables';

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

const memoryItemFormRef: Ref<MemoryItemFormExpose | null> = ref(null);

const { t } = useI18n();

const { memoryItem, isLoading, isUpdatingMemoryItem, handleUpdateItem, closeForm } = useMemoryItemEdit({
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