<template>
  <v-container fluid>
    <v-row>
      <v-col id="family-auto-complete" cols="12" md="4">
        <family-auto-complete v-model="selectedFamilyId" :label="t('dashboard.filterByFamily')" clearable hide-details
          prepend-inner-icon="mdi-filter-variant" />
      </v-col>
    </v-row>

    <!-- Top Summary Section -->
    <v-row>
      <v-col cols="12">
        <DashboardStats :family-id="selectedFamilyId || undefined" id="dashboard-stats" />
      </v-col>
    </v-row>

    <!-- Bottom Section: Family Tree Overview -->
    <!-- <v-row id="genealogy-chart">
      <v-col cols="12">
        <FamilyTreeOverview :family-id="selectedFamilyId" />
      </v-col>
    </v-row> -->

    <!-- Middle Section: Recent Activity -->
    <v-row>
      <v-col cols="12" md="6">
        <EventCalendar :family-id="selectedFamilyId || undefined" :read-only="true" id="dashboard-event-calendar" />
      </v-col>
      <v-col cols="12" md="6">
        <RecentActivity :family-id="selectedFamilyId || undefined" id="dashboard-recent-activity" />
      </v-col>
    </v-row>

  </v-container>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useDashboardStore } from '@/stores/dashboard.store';
import DashboardStats from '@/components/dashboard/DashboardStats.vue';
import RecentActivity from '@/components/dashboard/RecentActivity.vue';
import EventCalendar from '@/components/event/EventCalendar.vue';
import { useOnboardingTour } from '@/composables';

const { t } = useI18n();
useOnboardingTour();

const dashboardStore = useDashboardStore();

const selectedFamilyId = ref<string | null>(null);

onMounted(async () => {
  dashboardStore.fetchAllDashboardData();
});

watch(selectedFamilyId, (newFamilyId) => {
  dashboardStore.fetchAllDashboardData(newFamilyId || undefined);
});
</script>

<style scoped>
.btn-preview {
  height: 100%;
}
</style>