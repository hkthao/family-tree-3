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
            <v-btn color="primary" @click="exportData(familyId)" :loading="isExportingFamilyData">
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
            <v-btn color="primary" @click="importData" :loading="isImportingFamilyData"
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
import { useI18n } from 'vue-i18n';
import { useFamilyDataManagement } from '@/composables';
import PrivacySettings from '@/components/family/PrivacySettings.vue'; // Import the new component

const { t } = useI18n();

const props = defineProps<{
  familyId: string;
}>();

const {
  state: { importFile, clearExistingData, isExportingFamilyData, isImportingFamilyData },
  actions: { exportData, importData },
} = useFamilyDataManagement(props.familyId);
</script>

<style scoped>
/* Add any specific styles here */
</style>