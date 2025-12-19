import { describe, it, expect, vi, beforeEach } from 'vitest';
import { computed, ref } from 'vue';
import { useGenderRatioChart } from '@/composables/charts/useGenderRatioChart';
import { useI18n } from 'vue-i18n';
import { useTheme } from 'vuetify';

// Mock Vue-i18n
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(),
}));

// Mock Vuetify useTheme
vi.mock('vuetify', () => ({
  useTheme: vi.fn(),
}));

describe('useGenderRatioChart', () => {
  const mockT = vi.fn((key: string) => key);
  const mockTheme = {
    global: {
      current: ref({
        colors: {
          primary: '#1976D2',
          error: '#FF5252',
          'on-surface': '#000000',
        },
      }),
    },
  };

  beforeEach(() => {
    vi.clearAllMocks();
    (useI18n as vi.Mock).mockReturnValue({ t: mockT });
    (useTheme as vi.Mock).mockReturnValue(mockTheme);
  });

  // Test case 1: Series calculation with valid ratios
  it('should correctly calculate series with valid male and female ratios', () => {
    const props = { maleRatio: 0.6, femaleRatio: 0.4 };
    const { series } = useGenderRatioChart(props);

    expect(series.value).toEqual([60, 40]);
  });

  // Test case 2: Series calculation with undefined ratios
  it('should return [0, 0] for series if male or female ratios are undefined', () => {
    const props1 = { maleRatio: undefined, femaleRatio: 0.4 };
    const { series: series1 } = useGenderRatioChart(props1);
    expect(series1.value).toEqual([0, 0]);

    const props2 = { maleRatio: 0.6, femaleRatio: undefined };
    const { series: series2 } = useGenderRatioChart(props2);
    expect(series2.value).toEqual([0, 0]);

    const props3 = { maleRatio: undefined, femaleRatio: undefined };
    const { series: series3 } = useGenderRatioChart(props3);
    expect(series3.value).toEqual([0, 0]);
  });

  // Test case 3: Chart options structure and dynamic values
  it('should return correct chartOptions with dynamic labels and colors', () => {
    const props = { maleRatio: 0.5, femaleRatio: 0.5 };
    const { chartOptions } = useGenderRatioChart(props);

    // Assert labels
    expect(chartOptions.value.labels).toEqual(['member.gender.male', 'member.gender.female']);
    expect(mockT).toHaveBeenCalledWith('member.gender.male');
    expect(mockT).toHaveBeenCalledWith('member.gender.female');

    // Assert colors
    expect(chartOptions.value.colors).toEqual([mockTheme.global.current.value.colors.primary, mockTheme.global.current.value.colors.error]);

    // Assert total label and formatter
    expect(chartOptions.value.plotOptions?.pie?.donut?.labels?.total?.label).toBe('dashboard.genderRatioChart.total');
    expect(mockT).toHaveBeenCalledWith('dashboard.genderRatioChart.total');

    const mockW = {
      globals: {
        seriesTotals: [50, 50],
      },
    };
    const totalFormatter = chartOptions.value.plotOptions?.pie?.donut?.labels?.total?.formatter as Function;
    expect(totalFormatter(mockW)).toBe('100.0%');

    // Assert dataLabels formatter
    const dataLabelFormatter = chartOptions.value.dataLabels?.formatter as Function;
    expect(dataLabelFormatter(12.345)).toBe('12.3%');
  });

  // Test case 4: Chart options for dark theme (ensuring color properties are correctly assigned)
  it('should apply on-surface color to legend and dataLabels for theme awareness', () => {
    const props = { maleRatio: 0.5, femaleRatio: 0.5 };
    const { chartOptions } = useGenderRatioChart(props);

    // Legend labels color
    expect(chartOptions.value.legend?.labels?.colors).toBe(mockTheme.global.current.value.colors['on-surface']);

    // Data labels style color
    expect(chartOptions.value.dataLabels?.style?.colors).toEqual([mockTheme.global.current.value.colors['on-surface']]);

    // Total value color
    expect(chartOptions.value.plotOptions?.pie?.donut?.labels?.total?.color).toBe(mockTheme.global.current.value.colors['on-surface']);
  });
});
