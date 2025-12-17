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
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm" :disabled="isAddingMemoryItem">{{
        t('common.cancel')
        }}</v-btn>
      <v-btn color="primary" data-testid="button-save" @click="handleAddItem" :loading="isAddingMemoryItem"
        :disabled="isAddingMemoryItem">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import MemoryItemForm from '@/components/memory-item/MemoryItemForm.vue';
import type { MemoryItem } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useAddMemoryItemMutation } from '@/composables/memory-item';

const { familyId } = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
});

const memoryItemFormRef = ref<InstanceType<typeof MemoryItemForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const { mutate: addMemoryItem, isPending: isAddingMemoryItem } = useAddMemoryItemMutation();

const emit = defineEmits(['close', 'saved']);

const handleAddItem = async () => {
  if (!memoryItemFormRef.value) return;
  const isValid = await memoryItemFormRef.value.validate();
  if (!isValid) return;
  const itemData = memoryItemFormRef.value.getFormData();
  addMemoryItem(itemData as Omit<MemoryItem, 'id'>, {
    onSuccess: () => {
      showSnackbar(
        t('memoryItem.messages.addSuccess'),
        'success',
      );
      emit('close');
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