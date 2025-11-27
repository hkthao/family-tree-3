<template>
  <v-card-title class="text-center">
    <span class="text-h6">{{ t('memory.edit.title') }}</span>
  </v-card-title>
  <MemoryForm
    v-if="editedMemory"
    ref="memoryFormRef"
    v-model="editedMemory"
    :member-id="editedMemory.memberId"

    :readonly="false"
  />
  <v-card-text v-else>
    <v-alert type="info">{{ t('memory.edit.loading') }}</v-alert>
  </v-card-text>
  <v-card-actions>
    <v-spacer></v-spacer>
    <v-btn color="blue-darken-1" variant="text" @click="handleClose">
      {{ t('common.cancel') }}
    </v-btn>
    <v-btn color="blue-darken-1" variant="text" @click="handleSave" :loading="isSaving">
      {{ t('common.save') }}
    </v-btn>

  </v-card-actions>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemoryDto } from '@/types/memory'; // Removed UpdateMemoryDto
import MemoryForm from '@/components/memory/MemoryForm.vue';

const props = defineProps<{
  memoryId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const { showSnackbar } = useGlobalSnackbar();

const memoryFormRef = ref<InstanceType<typeof MemoryForm> | null>(null);
const editedMemory = ref<MemoryDto | null>(null); // Changed to MemoryDto

const isSaving = ref(false); // To manage loading state for buttons

const handleSave = async () => {
  if (!editedMemory.value || !memoryFormRef.value) return;

  isSaving.value = true;
  try {
    if (!memoryFormRef.value.isValid) {
      isSaving.value = false;
      showSnackbar(t('common.validations.required'), 'error'); // Generic validation error message
      return;
    }



    const result = await memoryStore.updateItem(editedMemory.value);
    if (result.ok) {
      showSnackbar(t('memory.edit.saveSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(t('memory.edit.saveFailed'), 'error');
    }
  } catch (error) {
    console.error('Error saving memory:', error);
    // Corrected showSnackbar call: message as first arg, color as second
    showSnackbar((error as Error).message, 'error');
  } finally {
    isSaving.value = false;
  }
};

const handleClose = () => {
  emit('close');
};
</script>