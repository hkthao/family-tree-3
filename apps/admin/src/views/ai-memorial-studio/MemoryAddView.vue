<template>
  <v-card-title class="text-center">
    <span class="text-h6">{{ t('memory.create.title') }}</span>
  </v-card-title>
  <MemoryForm
    ref="memoryFormRef"
    v-model="editedMemory"
    :member-id="memberId"

    :readonly="false"
  />
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
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemoryDto } from '@/types/memory';
import MemoryForm from '@/components/memory/MemoryForm.vue';

const props = defineProps<{
  memberId?: string; // Optional memberId for pre-filling
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const { showSnackbar } = useGlobalSnackbar();

const memoryFormRef = ref<InstanceType<typeof MemoryForm> | null>(null);

const isSaving = ref(false); // To manage loading state for buttons

const editedMemory = ref<MemoryDto>({
  memberId: props.memberId || '', // Pre-fill if memberId is provided
  title: '',
  rawInput: '', // Changed from undefined to ''
  story: undefined, // Added new field
  storyStyle: 'nostalgic', // NEW: Initialize storyStyle
  photoUrl: undefined, // This will temporarily hold a file name if files are selected
  tags: [],
  keywords: [],
  eventSuggestion: undefined,
  customEventDescription: undefined,
  emotionContextTags: [],
  customEmotionContext: undefined,
  faces: [],
  id: undefined, // MemoryDto specific
  createdAt: undefined, // MemoryDto specific
});

const handleSave = async () => {
  if (!memoryFormRef.value) return;

  isSaving.value = true;
  try {
    if (!memoryFormRef.value.isValid) {
      isSaving.value = false;
      showSnackbar(t('common.validations.required'), 'error'); // Generic validation error message
      return;
    }



    const result = await memoryStore.addItem(editedMemory.value);
    if (result.ok) {
      showSnackbar(t('memory.create.step5.saveSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(t('memory.create.step5.saveFailed'), 'error');
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
