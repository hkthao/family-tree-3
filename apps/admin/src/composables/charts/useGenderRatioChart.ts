import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from 'vuetify';

interface GenderRatioChartProps {
  maleRatio: number | undefined;
  femaleRatio: number | undefined;
}

export function useGenderRatioChart(props: GenderRatioChartProps) {
  const { t } = useI18n();
  const theme = useTheme();

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
      width: '100%',
    },
    labels: [t('member.gender.male'), t('member.gender.female')],
    colors: [theme.global.current.value.colors.primary, theme.global.current.value.colors.error],
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
              colors: theme.global.current.value.colors['on-surface'],
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
        colors: [theme.global.current.value.colors['on-surface']],
      },
    },
    legend: {
      position: 'bottom',
      labels: {
        colors: theme.global.current.value.colors['on-surface'],
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
              color: theme.global.current.value.colors['on-surface'],
            },
            value: {
              color: theme.global.current.value.colors['on-surface'],
            }
          },
        },
      },
    },
  }));

  return {
    series,
    chartOptions,
  };
}
