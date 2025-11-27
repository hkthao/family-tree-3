<template>
  <v-card-title class="text-center">
    <span class="text-h6">{{ t('memory.create.title') }}</span>
  </v-card-title>
  <MemoryForm
    ref="memoryFormRef"
    v-model="editedMemory"
    :member-id="memberId"
    @update:selectedFiles="handleSelectedFilesUpdate"
    :readonly="false"
  />
  <v-card-actions>
    <v-spacer></v-spacer>
    <v-btn color="blue-darken-1" variant="text" @click="handleClose">
      {{ t('common.cancel') }}
    </v-btn>
    <v-btn v-if="memoryFormValidAndLoaded && currentStep > 1 && !isSaving" color="blue-darken-1" variant="text" @click="memoryFormRef?.prevStep()">
      {{ t('common.back') }}
    </v-btn>
    <v-btn v-if="memoryFormValidAndLoaded && currentStep < 3"
      color="blue-darken-1" variant="text"
      @click="memoryFormRef?.nextStep()"
      :loading="isSaving || (currentStep === 1 && isStep1Processing)"
      :disabled="isSaving || (currentStep === 1 && isStep1Processing)">
      {{ t('common.next') }}
    </v-btn>
    <v-btn v-else color="blue-darken-1" variant="text" @click="handleSave" :loading="isSaving">
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
const selectedFiles = ref<File[]>([]);
const isSaving = ref(false); // To manage loading state for buttons

const editedMemory = ref<MemoryDto>({
  memberId: props.memberId || '', // Pre-fill if memberId is provided
  title: '',
  rawInput: undefined, // Changed from story to rawInput and made optional
  story: undefined, // Added new field
  storyStyle: 'nostalgic', // NEW: Initialize storyStyle
  photoAnalysisId: undefined,
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

// Computed property for safe access to activeStep
const currentStep = computed(() => memoryFormRef.value?.activeStep ?? 1);

// Computed property to check if memoryFormRef is loaded and valid
const memoryFormValidAndLoaded = computed(() => memoryFormRef.value !== null);

// NEW: Computed property to check if Step 1 (Photo Upload) is currently processing
const isStep1Processing = computed(() => {
  // Access the isValid property exposed by MemoryStep1PhotoUpload
  // If step1Ref is null, assume not processing
  return currentStep.value === 1 && memoryFormRef.value?.step1Ref?.isValid === false;
});

// Watch for changes in memberId prop to update editedMemory
watch(() => props.memberId, (newMemberId) => {
  if (newMemberId) {
    editedMemory.value.memberId = newMemberId;
  }
}, { immediate: true });

const handleSelectedFilesUpdate = (files: File[]) => {
  selectedFiles.value = files;
};

const handleSave = async () => {
  if (!memoryFormRef.value) return;

  isSaving.value = true;
  try {
    // Validate the current step (Step 3 has no form, so validation is conceptual)
    // We need to ensure all previous steps are valid before final save
    const step1Valid = await memoryFormRef.value.validateStep(1);
    const step2Valid = await memoryFormRef.value.validateStep(2);

    if (!step1Valid || !step2Valid) { // Ensure all relevant steps are valid
        isSaving.value = false;
        showSnackbar(t('common.validations.required'), 'error'); // Generic validation error message
        return;
    }

    // Placeholder for file upload logic
    if (selectedFiles.value.length > 0) {
      console.log('Files to upload:', selectedFiles.value);
      // In a real application, you would upload files to a server here.
      // For now, we'll just assign the name of the first selected file as photoUrl
      editedMemory.value.photoUrl = selectedFiles.value[0]?.name || undefined;
      // Corrected showSnackbar call: message as first arg, color as second
      showSnackbar(`${selectedFiles.value.length} files selected. Upload logic to be implemented.`, 'info');
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
