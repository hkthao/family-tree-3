<template>
    <v-row>
      <v-col cols="12" md="6">
        <v-card flat class="mb-4">
          <v-card-title class="d-flex align-center pe-2">
            <v-icon icon="mdi-database-export"></v-icon> &nbsp;{{ t('family.export.title') }}
          </v-card-title>
          <v-card-text>
            <p>{{ t('family.export.description') }}</p>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="exportFamilyData" :loading="familyDataStore.exporting">
              {{ t('family.export.button') }}
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
      <v-col cols="12" md="6">
        <v-card flat class="mb-4">
          <v-card-title class="d-flex align-center pe-2">
            <v-icon icon="mdi-database-import"></v-icon> &nbsp;{{ t('family.import.title') }}
          </v-card-title>
          <v-card-text>
            <p>{{ t('family.import.description') }}</p>
            <v-file-input v-model="importFile" :label="t('family.import.file_input_label')" accept=".json"
              class="mt-4" prepend-icon="mdi-paperclip" show-size counter></v-file-input>
            <v-checkbox v-model="clearExistingData" :label="t('family.import.clearExistingDataLabel')"></v-checkbox>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="importFamilyData" :loading="familyDataStore.importing"
              :disabled="!importFile">
              {{ t('family.import.button') }}
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
      <v-col cols="12">
        <PrivacySettings :family-id="familyId" />
      </v-col>
      <v-col cols="12">
        <v-card flat class="mb-4">
          <v-card-title class="d-flex align-center pe-2">
            <v-icon icon="mdi-link-variant"></v-icon> &nbsp;{{ t('familyLink.title') }}
          </v-card-title>
          <v-card-text>
            <p>{{ t('familyLink.description') }}</p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyDataStore } from '@/stores/family-data.store';
import { useGlobalSnackbar } from '@/composables';
import type { FamilyExportDto } from '@/types/family';
import PrivacySettings from './PrivacySettings.vue'; // Import the new component

const { t } = useI18n();
const familyDataStore = useFamilyDataStore();
const { showSnackbar } = useGlobalSnackbar();

const props = defineProps<{
  familyId: string;
}>();

const importFile = ref<File | null>(null);
const clearExistingData = ref(true);

const exportFamilyData = async () => {
  const success = await familyDataStore.exportFamilyData(props.familyId);
  if (success) {
    showSnackbar(t('family.export.success'), 'success');
  } else {
    showSnackbar(`${t('family.export.error')}: ${familyDataStore.error}`, 'error');
  }
};

const importFamilyData = async () => {
  if (!importFile.value) return;

  try {
    const file = importFile.value;
    const reader = new FileReader();

    reader.onload = async (e) => {
      try {
        const fileContent = e.target?.result as string;
        const familyData: FamilyExportDto = JSON.parse(fileContent);
        const newFamilyId = await familyDataStore.importFamilyData(props.familyId, familyData, clearExistingData.value);

        if (newFamilyId) {
          showSnackbar(`${t('family.import.success')}: ${newFamilyId}`, 'success');
          importFile.value = null; // Clear file input
        } else {
          showSnackbar(`${t('family.import.error')}: ${familyDataStore.error}`, 'error');
        }
      } catch (parseError) {
        console.error('Error parsing JSON file:', parseError);
        showSnackbar(t('family.import.parse_error'), 'error');
      }
    };

    reader.onerror = (e) => {
      console.error('Error reading file:', e);
      showSnackbar(t('family.import.read_error'), 'error');
    };

    reader.readAsText(file);
  } catch (error) {
    console.error('Error importing family data:', error);
    showSnackbar(t('family.import.error'), 'error');
  }
};
</script>

<style scoped>
/* Add any specific styles here */
</style>
