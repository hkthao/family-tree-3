<template>
  <v-card-title class="text-center">
    <span class="text-h6">{{ t('memory.edit.title') }}</span>
  </v-card-title>
  <MemoryForm
    v-if="editedMemory"
    ref="memoryFormRef"
    v-model="editedMemory"
    :member-id="editedMemory.memberId"
    @update:selectedFiles="handleSelectedFilesUpdate"
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
    <v-btn v-if="memoryFormValidAndLoaded && currentStep > 1 && !isSaving" color="blue-darken-1" variant="text" @click="memoryFormRef?.prevStep()">
      {{ t('common.back') }}
    </v-btn>
    <v-btn v-if="memoryFormValidAndLoaded && currentStep < 3" color="blue-darken-1" variant="text" @click="memoryFormRef?.nextStep()" :loading="isSaving">
      {{ t('common.next') }}
    </v-btn>
    <v-btn v-else color="blue-darken-1" variant="text" @click="handleSave" :loading="isSaving">
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
const selectedFiles = ref<File[]>([]);
const isSaving = ref(false); // To manage loading state for buttons

// Computed property for safe access to activeStep
const currentStep = computed(() => memoryFormRef.value?.activeStep ?? 1);

// Computed property to check if memoryFormRef is loaded and valid
const memoryFormValidAndLoaded = computed(() => memoryFormRef.value !== null);

const fetchMemory = async (id: string) => {
  const memory = await memoryStore.getById(id);
  if (memory) {
    editedMemory.value = memory; // Direct assignment, as editedMemory is now MemoryDto
  } else {
    showSnackbar(t('memory.edit.notFound'), 'error');
    emit('close');
  }
};

onMounted(() => {
  if (props.memoryId) {
    fetchMemory(props.memoryId);
  }
});

watch(() => props.memoryId, (newId) => {
  if (newId) {
    fetchMemory(newId);
  }
});

const handleSelectedFilesUpdate = (files: File[]) => {
  selectedFiles.value = files;
};

const handleSave = async () => {
  if (!editedMemory.value || !memoryFormRef.value) return;

  isSaving.value = true;
  try {
    // Validate all steps before saving
    const step1Valid = await memoryFormRef.value.validateStep(1);
    const step2Valid = await memoryFormRef.value.validateStep(2);

    if (!step1Valid || !step2Valid) {
        isSaving.value = false;
        showSnackbar(t('common.validations.required'), 'error'); // Generic validation error message
        return;
    }

    // Placeholder for file upload logic (if new files are selected)
    if (selectedFiles.value.length > 0) {
      console.log('Files to upload for edit:', selectedFiles.value);
      // In a real application, you would upload files to a server here.
      // For now, we'll just assign the name of the first selected file as photoUrl
      editedMemory.value.photoUrl = selectedFiles.value[0]?.name || undefined;
      // Corrected showSnackbar call: message as first arg, color as second
      showSnackbar(`${selectedFiles.value.length} files selected for upload. Upload logic to be implemented.`, 'info');
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