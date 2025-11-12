<template>
  <v-container>
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
  </v-container>
</template>

<script setup lang="ts">
// No specific script logic needed for this page yet.
</script>

<style scoped>
/* Add any specific styles for the About page here */
</style>
