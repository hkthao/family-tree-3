<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 text-primary mb-4">{{ $t('onboarding.welcomeMessage') }}</h1>
        <p class="text-body-1">
          {{ $t('onboarding.description') }}
        </p>
      </v-col>
    </v-row>

    <v-row class="mt-6">
      <v-col cols="12">
        <v-card class="pa-4 text-center" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('onboarding.interactiveTour.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-2 mb-4">
              {{ $t('onboarding.interactiveTour.description') }}
            </p>
            <v-btn color="primary" @click="startTour" prepend-icon="mdi-play-circle-outline">
              {{ $t('onboarding.interactiveTour.startButton') }}
            </v-btn>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mt-6">
      <v-col cols="12">
        <v-card class="pa-4" flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('onboarding.whatYoullLearn.title') }}</v-card-title>
          <v-card-text>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-view-dashboard-outline">
                <v-list-item-title>{{ $t('onboarding.whatYoullLearn.dashboardOverview.title') }}</v-list-item-title>
                <v-list-item-subtitle>{{ $t('onboarding.whatYoullLearn.dashboardOverview.subtitle') }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-account-plus-outline">
                <v-list-item-title>{{ $t('onboarding.whatYoullLearn.memberCreation.title') }}</v-list-item-title>
                <v-list-item-subtitle>{{ $t('onboarding.whatYoullLearn.memberCreation.subtitle') }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-graph-outline">
                <v-list-item-title>{{ $t('onboarding.whatYoullLearn.genealogyChart.title') }}</v-list-item-title>
                <v-list-item-subtitle>{{ $t('onboarding.whatYoullLearn.genealogyChart.subtitle') }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-link-variant">
                <v-list-item-title>{{ $t('onboarding.whatYoullLearn.relationshipEditor.title') }}</v-list-item-title>
                <v-list-item-subtitle>{{ $t('onboarding.whatYoullLearn.relationshipEditor.subtitle') }}</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { driver } from 'driver.js';
import 'driver.js/dist/driver.css';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const startTour = () => {
  const driverObj = driver({
    showProgress: true,
    steps: [
      { element: '#dashboard-link', popover: { title: t('onboarding.tourSteps.dashboard.title'), description: t('onboarding.tourSteps.dashboard.description') } },
      { element: '#add-member-button', popover: { title: t('onboarding.tourSteps.addMember.title'), description: t('onboarding.tourSteps.addMember.description') } },
      { element: '#genealogy-chart', popover: { title: t('onboarding.tourSteps.genealogyChart.title'), description: t('onboarding.tourSteps.genealogyChart.description') } },
      { element: '#relationship-editor', popover: { title: t('onboarding.tourSteps.relationshipEditor.title'), description: t('onboarding.tourSteps.relationshipEditor.description') } },
    ]
  });
  driverObj.drive();
};

onMounted(() => {
  // You might want to automatically start the tour for new users,
  // or keep it manual as implemented above.
});
</script>

<style scoped>
/* Add any specific styles for the Onboarding page here */
</style>
