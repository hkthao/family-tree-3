<template>
  <v-container>
    <!-- About Section -->
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 text-primary mb-4">{{ $t('about.title') }}</h1>
        <p class="text-body-1">
          {{ $t('about.welcomeMessage') }}
          {{ $t('about.description') }}
        </p>
      </v-col>
    </v-row>

    <v-row class="mt-6">
      <v-col cols="12" md="6">
        <v-card class="h-100" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('about.features.title') }}</v-card-title>
          <v-card-text>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-robot-happy-outline">
                <v-list-item-title>{{ $t('about.features.aiAssistedCreation.title') }}</v-list-item-title>
                <v-list-item-subtitle>{{ $t('about.features.aiAssistedCreation.subtitle') }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-graph-outline">
                <v-list-item-title>{{ $t('about.features.visualCharts.title') }}</v-list-item-title>
                <v-list-item-subtitle>{{ $t('about.features.visualCharts.subtitle') }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-check-decagram-outline">
                <v-list-item-title>{{ $t('about.features.relationshipValidation.title') }}</v-list-item-title>
                <v-list-item-subtitle>{{ $t('about.features.relationshipValidation.subtitle') }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-history">
                <v-list-item-title>{{ $t('about.features.historyTracking.title') }}</v-list-item-title>
                <v-list-item-subtitle>{{ $t('about.features.historyTracking.subtitle') }}</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="h-100" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('about.mission.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">
              {{ $t('about.mission.description') }}
            </p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12" md="6">
        <v-card class="h-100" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('about.credits.title') }}</v-card-title>
          <v-card-text>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-account-tie">
                <v-list-item-title class="text-wrap">{{ $t('about.credits.developer') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-brain">
                <v-list-item-title class="text-wrap">{{ $t('about.credits.aiIntegration') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-palette">
                <v-list-item-title class="text-wrap">{{ $t('about.credits.uiFramework') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-chart-bubble">
                <v-list-item-title class="text-wrap">{{ $t('about.credits.visualization') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-database">
                <v-list-item-title class="text-wrap">{{ $t('about.credits.dataEngine') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-heart-multiple-outline">
                <v-list-item-title class="text-wrap">{{ $t('about.credits.specialThanks') }}</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="h-100" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('about.contact.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">
              {{ $t('about.contact.description') }} <router-link to="/help">{{ $t('about.contact.helpPageLink') }}</router-link>
              {{ $t('common.or') }} {{ $t('about.contact.supportTeam') }}.
            </p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-divider class="my-8"></v-divider>

    <!-- Version Section -->
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
/* Add any specific styles for the combined page here */
</style>