<template>
  <div>
    <div v-if="dashboardStore.loading">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p class="mt-2">{{ t('dashboard.stats.loading') }}</p>
    </div>
    <v-alert v-else-if="dashboardStore.error" type="error" dense dismissible >
      {{ dashboardStore.error }}
    </v-alert>
    <v-row v-else-if="stats">
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="blue">mdi-account-group</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ stats.totalFamilies || 0 }}</div>
          <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.families') }}</div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="green">mdi-account-multiple</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ stats.totalMembers || 0 }}</div>
          <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.members') }}</div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="purple">mdi-link-variant</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ stats.totalRelationships || 0 }}</div>
          <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.relationships') }}</div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="orange">mdi-sitemap</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ stats.totalGenerations || 0 }}</div>
          <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.generations') }}</div>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { useDashboardStore } from '@/stores/dashboard.store';
import { onMounted, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  familyId: { type: String, default: null },
});

const dashboardStore = useDashboardStore();

const stats = computed(() => dashboardStore.stats);

const fetchStats = (familyId: string | null) => {
  dashboardStore.fetchDashboardStats(familyId || undefined);
};

onMounted(() => {
  fetchStats(props.familyId);
});

watch(() => props.familyId, (newFamilyId) => {
  fetchStats(newFamilyId);
});
</script>
