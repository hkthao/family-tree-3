import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useTheme } from 'vuetify';
import type { ApexOptions } from 'apexcharts';
import { getGenderRatioChartSeries, getGenderRatioChartOptions } from './genderRatioChart.logic';

interface GenderRatioChartProps {
  maleRatio: number | undefined;
  femaleRatio: number | undefined;
}

export function useGenderRatioChart(
  props: GenderRatioChartProps,
) {
  const { t } = useI18n();
  const theme = useTheme();

  const series = computed(() => getGenderRatioChartSeries(props.maleRatio, props.femaleRatio));

  const chartOptions = computed<ApexOptions>(() => getGenderRatioChartOptions(t, theme));

  return {
    state: {
      series,
      chartOptions,
    }
  };
}
