import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { ApexOptions } from 'apexcharts';
import { useTheme } from 'vuetify';
import { getMembersPerGenerationChartData, getMembersPerGenerationChartOptions } from './membersPerGenerationChart.logic';

interface MembersPerGenerationChartProps {
  membersPerGeneration: { [key: number]: number } | undefined;
}

export function useMembersPerGenerationChart(
  props: MembersPerGenerationChartProps,
) {
  const { t } = useI18n();
  const theme = useTheme();

  const chartData = computed(() => getMembersPerGenerationChartData(props.membersPerGeneration, t));

  const chartOptions = computed<ApexOptions>(() =>
    getMembersPerGenerationChartOptions(chartData.value.categories, t, theme)
  );

  return {
    state: {
      chartData,
      chartOptions,
    }
  };
}
