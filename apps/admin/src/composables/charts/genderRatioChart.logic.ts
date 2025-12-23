import type { ApexOptions } from 'apexcharts';

// Define a type for the translation function
type TranslateFunction = (key: string) => string;

interface ChartTheme {
  global: {
    current: {
      value: {
        colors: {
          primary: string;
          error: string;
          'on-surface': string;
          [key: string]: string; // Allow other color keys
        };
        dark: boolean;
      };
    };
  };
}

/**
 * Computes the series data for the gender ratio donut chart.
 * @param maleRatio The ratio of males (0-1).
 * @param femaleRatio The ratio of females (0-1).
 * @returns An array of percentages for male and female, or [0,0] if ratios are undefined.
 */
export function getGenderRatioChartSeries(maleRatio: number | undefined, femaleRatio: number | undefined): number[] {
  if (maleRatio === undefined || femaleRatio === undefined) {
    return [0, 0];
  }
  return [maleRatio * 100, femaleRatio * 100];
}

/**
 * Computes the ApexOptions for the gender ratio donut chart.
 * @param t The i18n translation function.
 * @param theme The Vuetify theme object.
 * @returns ApexOptions for the chart.
 */
export function getGenderRatioChartOptions(t: TranslateFunction, theme: ChartTheme): ApexOptions {
  return {
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
              colors: [theme.global.current.value.colors['on-surface']],
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
        colors: [theme.global.current.value.colors['on-surface']],
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
  };
}
