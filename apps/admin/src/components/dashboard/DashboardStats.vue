<template>
  <div>
    <div v-if="loading">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p class="mt-2">{{ t('dashboard.stats.loading') }}</p>
    </div>
    <v-alert v-else-if="!stats && !loading" type="info" dense>
      {{ t('common.noDataAvailable') }}
    </v-alert>
    <v-row v-else-if="stats">
      <v-col cols="12" sm="6" md="3">
        <v-card height="100%" class="pa-3 text-center" elevation="1">
          <div class="d-flex flex-column align-center justify-center fill-height">
            <v-icon size="40" color="blue">mdi-account-group</v-icon>
            <div class="text-h5 font-weight-bold mt-2">{{ stats.totalFamilies || 0 }}</div>
            <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.families') }}</div>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card height="100%" class="pa-3 text-center" elevation="1">
          <div class="d-flex flex-column align-center justify-center fill-height">
            <v-icon size="40" color="green">mdi-account-multiple</v-icon>
            <div class="text-h5 font-weight-bold mt-2">{{ stats.totalMembers || 0 }}</div>
            <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.members') }}</div>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card height="100%" class="pa-3 text-center" elevation="1">
          <div class="d-flex flex-column align-center justify-center fill-height">
            <v-icon size="40" color="purple">mdi-link-variant</v-icon>
            <div class="text-h5 font-weight-bold mt-2">{{ stats.totalRelationships || 0 }}</div>
            <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.relationships') }}</div>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card height="100%" class="pa-3 text-center" elevation="1">
          <div class="d-flex flex-column align-center justify-center fill-height">
            <v-icon size="40" color="orange">mdi-sitemap</v-icon>
            <div class="text-h5 font-weight-bold mt-2">{{ stats.totalGenerations || 0 }}</div>
            <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.generations') }}</div>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-row v-if="stats">
      <v-col cols="12" sm="6" md="3">
        <v-card height="100%" class="pa-3 text-center" elevation="1">
          <div class="d-flex flex-column align-center justify-center fill-height">
            <v-icon size="40" color="red">mdi-database</v-icon>
            <div class="text-h5 font-weight-bold mt-2">{{ (stats.usedStorageMb || 0).toFixed(0) }} / {{
              (stats.maxStorageMb || 0).toFixed(0) }}</div>
            <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.storageUsed') }} (MB)</div>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card height="100%" class="pa-3 text-center" elevation="1">
          <div class="d-flex flex-column align-center justify-center fill-height">
            <v-icon size="40" color="teal">mdi-account-multiple-outline</v-icon>
            <div class="text-h5 font-weight-bold mt-2">{{ stats.totalMembers || 0 }} / {{ stats.maxMembers || 0 }}</div>
            <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.membersLimit') }}</div>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card height="100%" class="pa-3 text-center" elevation="1">
          <div class="d-flex flex-column align-center justify-center fill-height">
            <v-icon size="40" color="blue-grey">mdi-robot</v-icon>
            <div class="text-h5 font-weight-bold mt-2">{{ stats.aiChatMonthlyUsage || 0 }} / {{ stats.aiChatMonthlyLimit
              || 0 }}</div>
            <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.aiChatMonthly') }}</div>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card height="100%" class="pa-3 text-center" elevation="1">
          <div class="d-flex flex-column align-center justify-center fill-height">
            <v-icon size="40" color="teal">mdi-calendar-check</v-icon>
            <div class="text-h5 font-weight-bold mt-2">{{ stats.upcomingEventsCount || 0 }}</div>
            <div class="text-subtitle-1 text-grey">{{ t('dashboard.stats.upcomingEvents') }}</div>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <!-- New Stats Section -->
    <v-row v-if="stats">
      <v-col cols="12" md="4">
        <GenderRatioChart class="fill-height" :male-ratio="stats.maleRatio" :female-ratio="stats.femaleRatio"
          :loading="loading" />
      </v-col>

      <v-col cols="12" md="8">
        <v-row>
          <v-col>
            <AverageAgeCard :average-age="stats.averageAge" :loading="loading" />
          </v-col>
          <v-col>
            <LivingDeceasedCard :living-members-count="stats.livingMembersCount"
              :deceased-members-count="stats.deceasedMembersCount" :loading="loading" />
          </v-col>
        </v-row>

        <v-row>
          <v-col>
            <MembersPerGenerationChart :members-per-generation="stats.membersPerGeneration" :loading="loading" />
          </v-col>
        </v-row>

      </v-col> <!-- Empty column to maintain md="4" x 3 layout if only 2 components are stacked -->
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { PropType } from 'vue';
import AverageAgeCard from './AverageAgeCard.vue';
import LivingDeceasedCard from './LivingDeceasedCard.vue';
import GenderRatioChart from './GenderRatioChart.vue';
import MembersPerGenerationChart from './MembersPerGenerationChart.vue';
import type { DashboardStats } from '@/types';

const { t } = useI18n();

defineProps({
  familyId: { type: String, default: null }, // Keep familyId for consistency if it's used elsewhere for context
  stats: { type: Object as PropType<DashboardStats | null>, default: null },
  loading: { type: Boolean, default: false },
});
</script>