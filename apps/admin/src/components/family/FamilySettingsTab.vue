<template>
  <v-card flat>
    <v-tabs v-model="tab" color="primary" align-tabs="center">
      <v-tab value="export-import">{{ t('family.settings.export_import_title') }}</v-tab>
      <v-tab value="privacy">{{ t('family.privacy.title') }}</v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <v-window-item value="export-import">
        <v-container>
          <v-row>
            <v-col cols="12" md="6">
              <v-card flat>
                <v-card-title>{{ t('family.export.title') }}</v-card-title>
                <v-card-text>
                  <p>{{ t('family.export.description') }}</p>
                </v-card-text>
                <v-card-actions>
                  <v-btn color="primary" @click="exportFamilyData" :loading="familyDataStore.exporting">
                    {{ t('family.export.button') }}
                  </v-btn>
                </v-card-actions>
              </v-card>
            </v-col>
            <v-col cols="12" md="6">
              <v-card flat>
                <v-card-title>{{ t('family.import.title') }}</v-card-title>
                <v-card-text>
                  <p>{{ t('family.import.description') }}</p>
                  <v-file-input v-model="importFile" :label="t('family.import.file_input_label')" accept=".json"
                    class="mt-4" prepend-icon="mdi-paperclip" show-size counter></v-file-input>
                </v-card-text>
                <v-card-actions>
                  <v-btn color="primary" @click="importFamilyData" :loading="familyDataStore.importing"
                    :disabled="!importFile">
                    {{ t('family.import.button') }}
                  </v-btn>
                </v-card-actions>
              </v-card>
            </v-col>
          </v-row>
        </v-container>
      </v-window-item>

      <v-window-item value="privacy">
        <PrivacySettings :family-id="familyId" />
      </v-window-item>
    </v-window>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyDataStore } from '@/stores/family-data.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { FamilyExportDto } from '@/types/family';
import PrivacySettings from './PrivacySettings.vue'; // Import the new component

const { t } = useI18n();
const familyDataStore = useFamilyDataStore();
const { showSnackbar } = useGlobalSnackbar();

const props = defineProps<{
  familyId: string;
}>();

const tab = ref('export-import'); // Default to export-import tab
const importFile = ref<File | null>(null);

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
        const newFamilyId = await familyDataStore.importFamilyData(familyData);

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
