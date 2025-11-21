<template>
  <v-dialog v-model="dialog" max-width="800px" persistent>
    <v-card>
      <v-card-title class="d-flex align-center">
        {{ t('familyDict.import.title') }}
      </v-card-title>
      <v-progress-linear v-if="familyDictStore.add.loading" indeterminate color="primary"></v-progress-linear>
      <v-card-text>
        <VFileUpload v-model="selectedFile" :label="t('familyDict.import.selectJsonFile')" accept=".json" show-size
          counter prepend-icon="mdi-json" @update:modelValue="onFileSelected" data-testid="json-file-input">
        </VFileUpload>

        <v-alert v-if="parsedDataError" type="error" class="mt-4">{{ parsedDataError }}</v-alert>

        <v-textarea v-if="parsedDataPreview" v-model="parsedDataPreview" :label="t('familyDict.import.preview')"
          readonly rows="10" class="mt-4"></v-textarea>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" @click="closeDialog" data-testid="import-cancel-button">{{ t('common.cancel') }}</v-btn>
        <v-btn color="primary" @click="importFamilyDicts" :loading="familyDictStore.add.loading"
          :disabled="!parsedData || !!parsedDataError || familyDictStore.add.loading" data-testid="import-save-button">
          {{ t('common.import') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyDictStore } from '@/stores/family-dict.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { FamilyDict } from '@/types';
import { VFileUpload } from 'vuetify/labs/VFileUpload';

const props = defineProps<{
  show: boolean;
}>();

const emit = defineEmits(['update:show', 'imported']);

const { t } = useI18n();
const familyDictStore = useFamilyDictStore();
const { showSnackbar } = useGlobalSnackbar();

const dialog = ref(props.show);
const selectedFile = ref<File[]>([]);
const parsedData = ref<Omit<FamilyDict, 'id'>[] | null>(null);
const parsedDataPreview = ref('');
const parsedDataError = ref('');

watch(() => props.show, (newVal) => {
  dialog.value = newVal;
  if (!newVal) {
    resetState(); // Reset form when dialog is closed
  }
});

watch(dialog, (newVal) => {
  emit('update:show', newVal);
});

const onFileSelected = (files: File[]) => {
  if (files && files.length > 0) {
    const file = files[0];
    if (file.type !== 'application/json') {
      parsedDataError.value = t('familyDict.import.errors.invalidFileType');
      parsedData.value = null;
      parsedDataPreview.value = '';
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const content = e.target?.result as string;
        const data = JSON.parse(content);

        // Basic validation for array of objects matching FamilyDict structure
        if (!Array.isArray(data) || data.some(item => !item.name || !item.type || !item.namesByRegion || !item.namesByRegion.north)) {
          parsedDataError.value = t('familyDict.import.errors.invalidJsonStructure');
          parsedData.value = null;
        } else {
          parsedData.value = data;
          parsedDataError.value = '';
        }
        parsedDataPreview.value = JSON.stringify(data, null, 2);
      } catch (error) {
        parsedDataError.value = t('familyDict.import.errors.invalidJson');
        parsedData.value = null;
        parsedDataPreview.value = '';
      }
    };
    reader.readAsText(file);
  } else {
    resetState();
  }
};

const importFamilyDicts = async () => {
  if (!parsedData.value) return;

  const result = await familyDictStore.importItems(parsedData.value);
  if (result.ok) {
    showSnackbar(t('familyDict.import.messages.importSuccess'), 'success');
    emit('imported');
    closeDialog();
  } else {
    showSnackbar(familyDictStore.error || t('familyDict.import.messages.importError'), 'error');
  }
};

const closeDialog = () => {
  dialog.value = false;
};

const resetState = () => {
  selectedFile.value = [];
  parsedData.value = null;
  parsedDataPreview.value = '';
  parsedDataError.value = '';
  familyDictStore.error = null; // Clear store error related to add/import
};
</script>