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
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import VueApexCharts from 'vue3-apexcharts';
import type { ApexOptions } from 'apexcharts'; // Import ApexOptions for type safety
import { useTheme } from 'vuetify'; // Import useTheme

const { t } = useI18n();
const theme = useTheme(); // Initialize useTheme

const props = defineProps<{
  maleRatio: number | undefined;
  femaleRatio: number | undefined;
  loading: boolean;
}>();

const apexchart = VueApexCharts;

const series = computed(() => {
  if (props.maleRatio === undefined || props.femaleRatio === undefined) {
    return [0, 0];
  }
  return [props.maleRatio * 100, props.femaleRatio * 100];
});

const chartOptions = computed<ApexOptions>(() => ({
  chart: {
    type: 'donut',
    height: 250,
  },
  labels: [t('member.gender.male'), t('member.gender.female')],
  colors: [theme.global.current.value.colors.primary, theme.global.current.value.colors.error], // Use theme colors
  responsive: [
    {
      breakpoint: 480,
      options: {
        chart: {
          width: 200,
        },
        legend: {
          position: 'bottom',
          labels: {
            colors: theme.global.current.value.colors['on-surface'], // Ensure legend text is visible
          },
        },
      },
    },
  ],
  dataLabels: {
    enabled: true,
    formatter: function (val: number) {
      return val.toFixed(1) + '%';
    },
    style: {
      colors: [theme.global.current.value.colors['on-surface']], // Ensure data labels are visible
    },
  },
  legend: {
    position: 'bottom',
    labels: {
      colors: theme.global.current.value.colors['on-surface'], // Ensure legend text is visible
    },
  },
  plotOptions: {
    pie: {
      donut: {
        labels: {
          show: true,
          total: {
            show: true,
            label: t('dashboard.genderRatioChart.total'),
            formatter: function (w: any) {
              const total = w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0);
              return total.toFixed(1) + '%';
            },
            color: theme.global.current.value.colors['on-surface'], // Ensure total label is visible
          },
          value: {
            color: theme.global.current.value.colors['on-surface'], // Ensure value label is visible
          }
        },
      },
    },
  },
}));
</script>

<style scoped>
/* Add any specific styles for the chart here */
</style>
