<template>
  <v-container fluid>
    <v-row >
      <v-col cols="12" md="4">
        <family-auto-complete
          v-model="selectedFamilyId"
          :label="t('dashboard.filterByFamily')"
          clearable
          hide-details
          prepend-inner-icon="mdi-filter-variant"
        />
      </v-col>
    </v-row>

    <!-- Top Summary Section -->
    <v-row>
      <v-col cols="12">
        <DashboardStats :family-id="selectedFamilyId || undefined" />
      </v-col>
    </v-row>

    <!-- Middle Section: Recent Activity -->
    <v-row>
      <v-col cols="12" md="6">
        <RecentActivity :family-id="selectedFamilyId || undefined" />
      </v-col>

      <v-col cols="12" md="6">
        <UpcomingEvents :family-id="selectedFamilyId || undefined" />
      </v-col>
    </v-row>

    <!-- Bottom Section: Family Tree Overview -->
    <v-row>
      <v-col cols="12">
        <FamilyTreeOverview :family-id="selectedFamilyId" />
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
import FamilyTreeOverview from '@/components/dashboard/FamilyTreeOverview.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import UpcomingEvents from '@/components/dashboard/UpcomingEvents.vue';

const { t } = useI18n();

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
/* Add any specific styles for the Dashboard page here */
</style>