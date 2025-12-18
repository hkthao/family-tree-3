<template>
  <v-card class="mb-4" elevation="2" style="transition: all 0.3s ease-in-out;" hover>
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-gender-male-female</v-icon>
      <span class="ml-2 text-high-emphasis font-weight-bold">{{ t('dashboard.genderRatioChart.title') }}</span>
      <v-spacer></v-spacer>
      <v-progress-circular v-if="loading" indeterminate size="24" color="primary"></v-progress-circular>
    </v-card-title>
    <v-card-text class="fill-height pa-3">
      <div class="d-flex flex-column align-center justify-center fill-height">
        <div v-if="loading" class="text-body-2 text-high-emphasis font-weight-bold">{{ t('dashboard.genderRatioChart.loading') }}</div>
        <div v-else-if="maleRatio === undefined || femaleRatio === undefined || (maleRatio === 0 && femaleRatio === 0)" class="text-body-2 font-weight-bold">
          {{ t('dashboard.genderRatioChart.noData') }}
        </div>
        <apexchart v-else type="donut" :options="chartOptions" :series="series"></apexchart>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import VueApexCharts from 'vue3-apexcharts';
import { useGenderRatioChart } from '@/composables';

const { t } = useI18n();

const props = defineProps<{
  maleRatio: number | undefined;
  femaleRatio: number | undefined;
  loading: boolean;
}>();

const apexchart = VueApexCharts;

const { series, chartOptions } = useGenderRatioChart(props);
</script>

<style scoped>
/* Add any specific styles for the chart here */
</style>
