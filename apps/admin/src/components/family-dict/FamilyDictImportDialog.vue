<template>
  <v-dialog v-model="dialog" max-width="800px" persistent>
    <v-card>
      <v-card-title class="d-flex align-center">
        {{ t('familyDict.import.title') }}
      </v-card-title>
      <v-progress-linear v-if="isImportingFamilyDicts" indeterminate color="primary"></v-progress-linear>
      <v-card-text>
        <VFileUpload clearable v-model="selectedFile" :label="t('familyDict.import.selectJsonFile')" accept=".json"
          show-size counter prepend-icon="mdi-json" @update:modelValue="onFileSelected" data-testid="json-file-input">
        </VFileUpload>

        <v-alert v-if="parsedDataError" type="error" class="mt-4">{{ parsedDataError }}</v-alert>


      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" @click="closeDialog" data-testid="import-cancel-button">{{ t('common.cancel') }}</v-btn>
        <v-btn color="primary" @click="importFamilyDicts" :loading="isImportingFamilyDicts"
          :disabled="!parsedData || !!parsedDataError || isImportingFamilyDicts" data-testid="import-save-button">
          {{ t('common.import') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import type { FamilyDict } from '@/types';
import { VFileUpload } from 'vuetify/labs/VFileUpload';
import { useImportFamilyDictMutation } from '@/composables/family-dict';


const props = defineProps<{
  show: boolean;
}>();

const emit = defineEmits(['update:show', 'imported']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const dialog = ref(props.show);
const selectedFile = ref<File[]>([]);
const parsedData = ref<Omit<FamilyDict, 'id'>[] | null>(null);
const parsedDataError = ref('');

const { mutate: importFamilyDictsMutation, isPending: isImportingFamilyDicts } = useImportFamilyDictMutation();

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
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const content = e.target?.result as string;
        const data = JSON.parse(content);
        if (!Array.isArray(data) || data.some(item => !item.name || Number.isNaN(item.type) || Number.isNaN(item.lineage) || !item.namesByRegion || !item.namesByRegion.north)) {
          parsedDataError.value = t('familyDict.import.errors.invalidJsonStructure');
          parsedData.value = null;
        } else {
          parsedData.value = data;
          parsedDataError.value = '';
        }
      } catch (error) {
        parsedDataError.value = t('familyDict.import.errors.invalidJson');
        parsedData.value = null;
      }
    };
    reader.readAsText(file);
  } else {
    resetState();
  }
};

const importFamilyDicts = async () => {
  if (!parsedData.value) return;

  importFamilyDictsMutation({
    familyDicts: parsedData.value as FamilyDict[]
  }, {
    onSuccess: () => {
      showSnackbar(t('familyDict.import.messages.importSuccess'), 'success');
      emit('imported');
      closeDialog();
    },
    onError: (error) => {
      showSnackbar(error.message || t('familyDict.import.messages.importError'), 'error');
    },
  });
};

const closeDialog = () => {
  dialog.value = false;
};

const resetState = () => {
  selectedFile.value = [];
  parsedData.value = null;
  parsedDataError.value = '';
};
</script>