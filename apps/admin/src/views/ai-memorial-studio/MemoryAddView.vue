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
    <v-btn v-if="memoryFormRef?.activeStep > 1 && !isSaving" color="blue-darken-1" variant="text" @click="memoryFormRef?.prevStep()">
      {{ t('common.back') }}
    </v-btn>
    <v-btn v-if="memoryFormRef?.activeStep < 3" color="blue-darken-1" variant="text" @click="memoryFormRef?.nextStep()" :loading="isSaving">
      {{ t('common.next') }}
    </v-btn>
    <v-btn v-else color="blue-darken-1" variant="text" @click="handleSave" :loading="isSaving">
      {{ t('common.save') }}
    </v-btn>
  </v-card-actions>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'; // Added 'computed'
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { CreateMemoryDto } from '@/types/memory';
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

const editedMemory = ref<CreateMemoryDto>({
  memberId: props.memberId || '', // Pre-fill if memberId is provided
  title: '',
  story: '',
  photoAnalysisId: undefined,
  photoUrl: undefined, // This will temporarily hold a file name if files are selected
  tags: [],
  keywords: [],
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
        return;
    }

    // Placeholder for file upload logic
    if (selectedFiles.value.length > 0) {
      console.log('Files to upload:', selectedFiles.value);
      // In a real application, you would upload files to a server here.
      // For now, we'll just assign the name of the first selected file as photoUrl
      editedMemory.value.photoUrl = selectedFiles.value[0]?.name || undefined;
      showSnackbar(t('common.info'), 'info', `${selectedFiles.value.length} files selected. Upload logic to be implemented.`);
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
    showSnackbar(t('common.error'), 'error', (error as Error).message);
  } finally {
    isSaving.value = false;
  }
};

const handleClose = () => {
  emit('close');
};
</script>
