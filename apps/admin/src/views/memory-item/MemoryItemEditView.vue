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
        :family-id="_familyId"
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
import { ref, computed, type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import MemoryItemForm from '@/components/memory-item/MemoryItemForm.vue';
import type { MemoryItem } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useMemoryItemQuery, useUpdateMemoryItemMutation } from '@/composables/memory-item';

interface MemoryItemFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => MemoryItem;
}

const { familyId: _familyId, memoryItemId: _memoryItemId } = defineProps({
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

const memoryItemFormRef = ref<MemoryItemFormExposed | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { data: memoryItem, isLoading: isLoadingMemoryItem } = useMemoryItemQuery(
  _familyId,
  _memoryItemId,
);
const { mutate: updateMemoryItem, isPending: isUpdatingMemoryItem } = useUpdateMemoryItemMutation();
const isLoading = computed(() => isLoadingMemoryItem.value);

const handleUpdateItem = async () => {
  if (!memoryItemFormRef.value) return;
  const isValid = await memoryItemFormRef.value.validate();
  if (!isValid) return;

  const itemData = memoryItemFormRef.value.getFormData();
  if (!itemData.id) {
    showSnackbar(
      t('memoryItem.messages.saveError'),
      'error',
    );
    return;
  }

  updateMemoryItem(itemData, {
    onSuccess: () => {
      showSnackbar(
        t('memoryItem.messages.updateSuccess'),
        'success',
      );
      emit('saved');
    },
    onError: (error) => {
      showSnackbar(
        error.message || t('memoryItem.messages.saveError'),
        'error',
      );
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>

<style scoped></style>