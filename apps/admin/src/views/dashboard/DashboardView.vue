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
        <DashboardStats :family-id="selectedFamilyId || undefined" :stats="dashboardStats" :loading="isLoadingStats" id="dashboard-stats" />
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
        <RecentActivity :family-id="selectedFamilyId || undefined" :activities="recentActivities" :loading="isLoadingRecentActivities" id="dashboard-recent-activity" />
      </v-col>
    </v-row>

  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import DashboardStats from '@/components/dashboard/DashboardStats.vue';
import RecentActivity from '@/components/dashboard/RecentActivity.vue';
import EventCalendar from '@/components/event/EventCalendar.vue';
import { useOnboardingTour } from '@/composables';
import { useDashboardStats, useRecentActivities } from '@/composables';
import { useAuthStore } from '@/stores/auth.store'; // Import auth store
import type { IFamilyAccess } from '@/types'; // Import IFamilyAccess type

const { t } = useI18n();
useOnboardingTour();

const authStore = useAuthStore(); // Use auth store

const selectedFamilyId = ref<string | null>(null);

// Watch for userFamilyAccess changes and set default selectedFamilyId
watch(() => authStore.userFamilyAccess, (newAccess: IFamilyAccess[]) => {
  if (newAccess && newAccess.length > 0 && !selectedFamilyId.value) {
    selectedFamilyId.value = newAccess[0].familyId;
  }
}, { immediate: true }); // immediate: true ensures this runs on component mount as well

const familyIdForQueries = computed(() => selectedFamilyId.value || undefined);

const { state: { dashboardStats, isLoading: isLoadingStats } } = useDashboardStats(familyIdForQueries);
const { state: { activities: recentActivities, isLoading: isLoadingRecentActivities } } = useRecentActivities(familyIdForQueries);
</script>

<style scoped>
.btn-preview {
  height: 100%;
}
</style>