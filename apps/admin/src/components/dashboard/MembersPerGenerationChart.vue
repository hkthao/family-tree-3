<template>
  <v-card elevation="2" style="transition: all 0.3s ease-in-out;" hover>
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-family-tree</v-icon>
      <span class="ml-2">{{ t('dashboard.membersPerGenerationChart.title') }}</span>
      <v-spacer></v-spacer>
      <v-progress-circular v-if="loading" indeterminate size="24" color="primary"></v-progress-circular>
    </v-card-title>
    <v-card-text class="fill-height">
      <div class="d-flex flex-column align-center justify-center fill-height">
        <div v-if="loading">{{ t('dashboard.membersPerGenerationChart.loading') }}</div>
        <div v-else-if="!chartData.series[0].data.length" class="text-medium-emphasis">
          {{ t('dashboard.membersPerGenerationChart.noData') }}
        </div>
        <apexchart v-else type="bar" :options="chartOptions" :series="chartData.series"></apexchart>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import VueApexCharts from 'vue3-apexcharts';
import type { ApexOptions } from 'apexcharts';

const { t } = useI18n();

const props = defineProps<{
  membersPerGeneration: { [key: number]: number } | undefined;
  loading: boolean;
}>();

const apexchart = VueApexCharts;

const chartData = computed(() => {
  if (!props.membersPerGeneration) {
    return {
      series: [{ name: t('dashboard.membersPerGenerationChart.members'), data: [] }],
      categories: [],
    };
  }

  const generations = Object.keys(props.membersPerGeneration)
    .map(Number)
    .sort((a, b) => a - b);
  const data = generations.map(gen => props.membersPerGeneration![gen]);

  return {
    series: [{ name: t('dashboard.membersPerGenerationChart.members'), data: data }],
    categories: generations.map(gen => `${t('dashboard.membersPerGenerationChart.generation')} ${gen}`),
  };
});

const chartOptions = computed<ApexOptions>(() => ({
  chart: {
    type: 'bar',
  },
  plotOptions: {
    bar: {
      horizontal: false,
      columnWidth: '55%',
      endingShape: 'rounded'
    },
  },
  dataLabels: {
    enabled: false
  },
  stroke: {
    show: true,
    width: 2,
    colors: ['transparent']
  },
  xaxis: {
    categories: chartData.value.categories,
    title: {
      text: t('dashboard.membersPerGenerationChart.generation')
    }
  },
  yaxis: {
    title: {
      text: t('dashboard.membersPerGenerationChart.totalMembers')
    }
  },
  fill: {
    opacity: 1
  },
  tooltip: {
    y: {
      formatter: function (val: number) {
        return `${val} ${t('dashboard.membersPerGenerationChart.members')}`
      }
    }
  },
  colors: ['#00E396'], // Example color for bars
}));
</script>

<style scoped>
/* Add any specific styles for the chart here */
</style>
