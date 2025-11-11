<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 text-primary mb-4">{{ $t('version.title') }}</h1>
        <p class="text-body-1">
          {{ $t('version.description') }}
        </p>
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12" md="6">
        <v-card class="h-100" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('version.currentVersion.title') }}</v-card-title>
          <v-card-text>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-web">
                <v-list-item-title>{{ $t('version.info.frontendVersion') }}</v-list-item-title>
                <v-list-item-subtitle>{{ frontendVersion }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-server">
                <v-list-item-title>{{ $t('version.info.backendVersion') }}</v-list-item-title>
                <v-list-item-subtitle>{{ backendVersion }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-calendar-clock-outline">
                <v-list-item-title>{{ $t('version.info.buildDate') }}</v-list-item-title>
                <v-list-item-subtitle>{{ buildDate }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-cloud-outline">
                <v-list-item-title>{{ $t('version.info.environment') }}</v-list-item-title>
                <v-list-item-subtitle>{{ environment }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-api">
                <v-list-item-title>{{ $t('version.info.apiEndpoint') }}</v-list-item-title>
                <v-list-item-subtitle>{{ apiEndpoint }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-git">
                <v-list-item-title>{{ $t('version.info.commitId') }}</v-list-item-title>
                <v-list-item-subtitle>{{ commitId }}</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="h-100" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('version.changelog.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">
              {{ $t('version.changelog.summary') }}
            </p>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-new-box">
                <v-list-item-title>{{ $t('version.changelog.feature1') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-bug">
                <v-list-item-title>{{ $t('version.changelog.bugfix1') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-palette-outline">
                <v-list-item-title>{{ $t('version.changelog.enhancement1') }}</v-list-item-title>
              </v-list-item>
            </v-list>
            <v-btn variant="text" color="primary" class="mt-4" href="#" target="_blank">
              {{ $t('version.changelog.viewFull') }} <v-icon right>mdi-open-in-new</v-icon>
            </v-btn>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import axios from 'axios'; // Assuming axios is available for API calls
import packageJson from '../../../package.json'; // To get frontend version

const frontendVersion = ref('N/A');
const backendVersion = ref('N/A');
const buildDate = ref('N/A');
const environment = ref('N/A');
const apiEndpoint = ref('N/A');
const commitId = ref('N/A');

onMounted(async () => {
  // Get frontend version from package.json
  frontendVersion.value = packageJson.version;

  // Get info from environment variables (Vite)
  buildDate.value = import.meta.env.VITE_APP_BUILD_DATE || 'N/A';
  environment.value = import.meta.env.VITE_APP_ENVIRONMENT || 'N/A';
  apiEndpoint.value = import.meta.env.VITE_API_BASE_URL || 'N/A';
  commitId.value = import.meta.env.VITE_APP_COMMIT_ID || 'N/A';

  // Fetch backend version
  try {
    const response = await axios.get(`${apiEndpoint.value}/version`); // Assuming an /api/version endpoint
    if (response.status === 200 && response.data && response.data.version) {
      backendVersion.value = response.data.version;
    } else {
      console.error('Failed to fetch backend version:', response.statusText);
    }
  } catch (error) {
    console.error('Error fetching backend version:', error);
    backendVersion.value = 'Error';
  }
});
</script>

<style scoped>
/* Add any specific styles for the Version page here */
</style>
