import type { ApexOptions } from 'apexcharts';

// Define a type for the translation function
type TranslateFunction = (key: string) => string;

interface ChartTheme {
  global: {
    current: {
      value: {
        colors: {
          primary: string;
          'on-surface': string;
          [key: string]: string; // Allow other color keys
        };
        dark: boolean;
      };
    };
  };
}

/**
 * Computes the chart data (series and categories) for the members per generation chart.
 * @param membersPerGeneration An object mapping generation numbers to member counts.
 * @param t The i18n translation function.
 * @returns An object containing series data and categories for the chart.
 */
export function getMembersPerGenerationChartData(
  membersPerGeneration: { [key: number]: number } | undefined,
  t: TranslateFunction
) {
  if (!membersPerGeneration) {
    return {
      series: [{ name: t('dashboard.membersPerGenerationChart.members'), data: [] }],
      categories: [],
    };
  }

  const generations = Object.keys(membersPerGeneration)
    .map(Number)
    .sort((a, b) => a - b);
  const data = generations.map(gen => membersPerGeneration[gen]);

  return {
    series: [{ name: t('dashboard.membersPerGenerationChart.members'), data: data }],
    categories: generations.map(gen => `${t('dashboard.membersPerGenerationChart.generation')} ${gen}`),
  };
}

/**
 * Computes the ApexOptions for the members per generation chart.
 * @param chartCategories The categories (generation labels) for the x-axis.
 * @param t The i18n translation function.
 * @param theme The Vuetify theme object.
 * @returns ApexOptions for the chart.
 */
export function getMembersPerGenerationChartOptions(
  chartCategories: string[],
  t: TranslateFunction,
  theme: ChartTheme
): ApexOptions {
  return {
    chart: {
      type: 'bar',
      foreColor: theme.global.current.value.colors['on-surface'],
    },
    plotOptions: {
      bar: {
        horizontal: false,
        columnWidth: '55%',
        borderRadiusApplication: 'around' // Corrected property
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
      categories: chartCategories,
      title: {
        text: t('dashboard.membersPerGenerationChart.generation'),
        style: {
          color: theme.global.current.value.colors['on-surface'], // Changed back to single string
        },
      },
      labels: {
        style: {
          colors: theme.global.current.value.colors['on-surface'], // Changed back to single string
        },
      },
    },
    yaxis: {
      title: {
        text: t('dashboard.membersPerGenerationChart.totalMembers'),
        style: {
          color: theme.global.current.value.colors['on-surface'], // Changed back to single string
        },
      },
      labels: {
        style: {
          colors: theme.global.current.value.colors['on-surface'], // Changed back to single string
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
      }
    },
    colors: [theme.global.current.value.colors.primary],
  };
}
