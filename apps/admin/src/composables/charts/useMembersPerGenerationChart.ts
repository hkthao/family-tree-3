import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from 'vuetify';

interface MembersPerGenerationChartProps {
  membersPerGeneration: { [key: number]: number } | undefined;
}

export function useMembersPerGenerationChart(props: MembersPerGenerationChartProps) {
  const { t } = useI18n();
  const theme = useTheme();

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
      foreColor: theme.global.current.value.colors['on-surface'],
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
        text: t('dashboard.membersPerGenerationChart.generation'),
        style: {
          color: theme.global.current.value.colors['on-surface'],
        },
      },
      labels: {
        style: {
          colors: theme.global.current.value.colors['on-surface'],
        },
      },
    },
    yaxis: {
      title: {
        text: t('dashboard.membersPerGenerationChart.totalMembers'),
        style: {
          color: theme.global.current.value.colors['on-surface'],
        },
      },
      labels: {
        style: {
          colors: theme.global.current.value.colors['on-surface'],
        },
      },
    },
    fill: {
      opacity: 1
    },
    tooltip: {
      theme: theme.global.current.value.dark ? 'dark' : 'light',
      y: {
        formatter: function (val: number) {
          return `${val} ${t('dashboard.membersPerGenerationChart.members')}`
        },
      },
      style: {
        fontSize: '12px',
        fontFamily: 'inherit',
        color: theme.global.current.value.colors['on-surface'],
      }
    },
    colors: [theme.global.current.value.colors.primary],
  }));

  return {
    chartData,
    chartOptions,
  };
}
