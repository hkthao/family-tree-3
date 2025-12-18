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
import { useI18n } from 'vue-i18n';
import { VFileUpload } from 'vuetify/labs/VFileUpload';
import { useFamilyDictImport } from '@/composables';


const props = defineProps<{
  show: boolean;
}>();

const emit = defineEmits(['update:show', 'imported']);

const { t } = useI18n();

const {
  dialog,
  selectedFile,
  parsedData,
  parsedDataError,
  isImportingFamilyDicts,
  onFileSelected,
  importFamilyDicts,
  closeDialog,
} = useFamilyDictImport(props.show, emit);
</script>