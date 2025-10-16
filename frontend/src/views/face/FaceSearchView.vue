<template>
  <v-container fluid>
    <v-card class="pa-4" elevation="2">
      <v-card-title class="text-h5">{{ t('face.search.title') }}</v-card-title>
      <v-card-text>
        <FaceUploadInput @file-uploaded="handleFileUpload" :multiple="false" />

        <v-progress-linear
          v-if="faceStore.loading"
          indeterminate
          color="primary"
          class="my-4"
        ></v-progress-linear>

        <v-alert v-if="faceStore.error" type="error" class="my-4">{{ faceStore.error }}</v-alert>

        <div v-if="faceStore.faceSearchResults.length > 0" class="mt-4">
          <v-card-subtitle class="text-h6">{{ t('face.search.resultsTitle') }}</v-card-subtitle>
          <FaceResultList :results="faceStore.faceSearchResults" />
        </div>
        <v-alert v-else-if="!faceStore.loading && !faceStore.error && uploadedSearchImage" type="info" class="mt-4">
          {{ t('face.search.noResults') }}
        </v-alert>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFaceStore } from '@/stores/face.store';
import { FaceUploadInput, FaceResultList } from '@/components/face';

const { t } = useI18n();
const faceStore = useFaceStore();
const uploadedSearchImage = ref<File | null>(null);

const handleFileUpload = async (file: File) => {
  uploadedSearchImage.value = file;
  await faceStore.searchByFace(file);
};
</script>
