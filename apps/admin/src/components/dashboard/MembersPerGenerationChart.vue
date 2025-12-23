<template>
  <v-card elevation="2" style="transition: all 0.3s ease-in-out;" hover>
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-family-tree</v-icon>
      <span class="ml-2 text-high-emphasis font-weight-bold">{{ t('dashboard.membersPerGenerationChart.title') }}</span>
      <v-spacer></v-spacer>
      <v-progress-circular v-if="loading" indeterminate size="24" color="primary"></v-progress-circular>
    </v-card-title>
    <v-card-text class="fill-height">
      <div class="d-flex flex-column align-center justify-center fill-height">
        <div v-if="loading" class="text-body-2 text-high-emphasis font-weight-bold">{{ t('dashboard.membersPerGenerationChart.loading') }}</div>
        <div v-else-if="!state.chartData.value.series[0].data.length" class="text-body-2 font-weight-bold">
          {{ t('dashboard.membersPerGenerationChart.noData') }}
        </div>
        <apexchart width="500"  v-else type="bar" :options="state.chartOptions.value" :series="state.chartData.value.series"></apexchart>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import VueApexCharts from 'vue3-apexcharts';
import { useMembersPerGenerationChart } from '@/composables';

const { t } = useI18n();

const props = defineProps<{
  membersPerGeneration: { [key: number]: number } | undefined;
  loading: boolean;
}>();

const apexchart = VueApexCharts;

const { state } = useMembersPerGenerationChart(props);
</script>

<style scoped>
/* Add any specific styles for the chart here */
</style>
