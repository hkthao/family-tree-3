<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <h2 class="text-h5 mb-4">{{ t('family.settings.export_import_title') }}</h2>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <v-card class="pa-4">
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
        <v-card class="pa-4">
          <v-card-title>{{ t('family.import.title') }}</v-card-title>
          <v-card-text>
            <p>{{ t('family.import.description') }}</p>
            <v-file-input
              v-model="importFile"
              :label="t('family.import.file_input_label')"
              accept=".json"
              prepend-icon="mdi-paperclip"
              show-size
              counter
            ></v-file-input>
          </v-card-text>
          <v-card-actions>
            <v-btn color="primary" @click="importFamilyData" :loading="familyDataStore.importing" :disabled="!importFile">
              {{ t('family.import.button') }}
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyDataStore } from '@/stores/family-data.store';
import type { FamilyExportDto } from '@/types/family';

const { t } = useI18n();
const familyDataStore = useFamilyDataStore();

const props = defineProps<{
  familyId: string;
}>();

const importFile = ref<File | null>(null);

const exportFamilyData = async () => {
  const success = await familyDataStore.exportFamilyData(props.familyId);
  if (success) {
    alert(t('family.export.success'));
  } else {
    alert(`${t('family.export.error')}: ${familyDataStore.error}`);
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
          alert(`${t('family.import.success')}: ${newFamilyId}`);
          importFile.value = null; // Clear file input
        } else {
          alert(`${t('family.import.error')}: ${familyDataStore.error}`);
        }
      } catch (parseError) {
        console.error('Error parsing JSON file:', parseError);
        alert(t('family.import.parse_error'));
      }
    };

    reader.onerror = (e) => {
      console.error('Error reading file:', e);
      alert(t('family.import.read_error'));
    };

    reader.readAsText(file);
  } catch (error) {
    console.error('Error importing family data:', error);
    alert(t('family.import.error'));
  }
};
</script>

<style scoped>
/* Add any specific styles here */
</style>
