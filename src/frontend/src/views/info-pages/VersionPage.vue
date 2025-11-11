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

    <v-row class="mt-6">
      <v-col cols="12" md="6">
        <v-card class="pa-4 h-100" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('version.currentVersion.title') }}</v-card-title>
          <v-card-text>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-tag-outline">
                <v-list-item-title>{{ $t('version.currentVersion.number') }}</v-list-item-title>
                <v-list-item-subtitle>{{ appVersion }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-calendar-clock-outline">
                <v-list-item-title>{{ $t('version.currentVersion.lastUpdated') }}</v-list-item-title>
                <v-list-item-subtitle>{{ lastUpdateDate }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-git">
                <v-list-item-title>{{ $t('version.currentVersion.gitCommitHash') }}</v-list-item-title>
                <v-list-item-subtitle>{{ gitCommitHash }}</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="pa-4 h-100" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('version.changelog.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-2">
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

const appVersion = ref('N/A');
const lastUpdateDate = ref('N/A');
const gitCommitHash = ref('N/A');

onMounted(async () => {
  try {
    const response = await fetch('/version.json');
    if (response.ok) {
      const data = await response.json();
      appVersion.value = data.version || 'N/A';
      lastUpdateDate.value = data.lastUpdate || 'N/A'; // Assuming version.json might contain this
      gitCommitHash.value = data.commitHash || 'N/A'; // Assuming version.json might contain this
    } else {
      console.error('Failed to fetch version.json:', response.statusText);
    }
  } catch (error) {
    console.error('Error fetching version.json:', error);
  }
});
</script>

<style scoped>
/* Add any specific styles for the Version page here */
</style>
