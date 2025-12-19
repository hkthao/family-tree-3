import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { useMembersPerGenerationChart } from '@/composables/charts/useMembersPerGenerationChart';
import * as chartLogic from '@/composables/charts/membersPerGenerationChart.logic';
import { useI18n } from 'vue-i18n';
import { useTheme } from 'vuetify';

// Mock vue-i18n and vuetify modules globally without factory functions
vi.mock('vue-i18n');
vi.mock('vuetify');

describe('useMembersPerGenerationChart', () => {
  const mockProps = {
    membersPerGeneration: { 1: 5, 2: 10 },
  };

  let getMembersPerGenerationChartDataSpy: ReturnType<typeof vi.spyOn>;
  let getMembersPerGenerationChartOptionsSpy: ReturnType<typeof vi.spyOn>;
  let mockT: ReturnType<typeof vi.fn>; // To store the 't' spy
  let mockThemeObject: any; // To store the mock theme object

  beforeEach(() => {
    vi.clearAllMocks(); // Clears all mocks, including those set by vi.mocked

    // Set up the mock for useI18n and capture the 't' spy
    mockT = vi.fn((key: string) => key);
    vi.mocked(useI18n).mockReturnValue({ t: mockT });

    // Set up the mock for useTheme and capture the mock theme object
    mockThemeObject = {
      global: {
        current: {
          value: {
            colors: {
              primary: '#1976D2',
              'on-surface': '#000000',
            },
            dark: false,
          },
        },
      },
    };
    vi.mocked(useTheme).mockReturnValue(mockThemeObject);

    getMembersPerGenerationChartDataSpy = vi.spyOn(chartLogic, 'getMembersPerGenerationChartData');
    getMembersPerGenerationChartOptionsSpy = vi.spyOn(chartLogic, 'getMembersPerGenerationChartOptions');

    getMembersPerGenerationChartDataSpy.mockImplementation((data, t) => {
      if (!data) return { series: [{ name: t('dashboard.membersPerGenerationChart.members'), data: [] }], categories: [] };
      const generations = Object.keys(data).map(Number).sort((a, b) => a - b);
      return {
        series: [{ name: t('dashboard.membersPerGenerationChart.members'), data: generations.map(gen => data[gen]) }],
        categories: generations.map(gen => `Gen ${gen}`),
      };
    });

    getMembersPerGenerationChartOptionsSpy.mockImplementation((categories, t, theme) => ({
      chart: { type: 'bar' },
      xaxis: { categories },
      colors: [theme.global.current.value.colors.primary],
    }));
  });

  afterEach(() => {
    getMembersPerGenerationChartDataSpy.mockRestore();
    getMembersPerGenerationChartOptionsSpy.mockRestore();
  });

  it('should return chartData from getMembersPerGenerationChartData', () => {
    const expectedChartData = { series: [{ name: 'members', data: [5, 10] }], categories: ['Gen 1', 'Gen 2'] };
    getMembersPerGenerationChartDataSpy.mockReturnValue(expectedChartData);
    const { state } = useMembersPerGenerationChart(mockProps);
    expect(state.chartData.value).toEqual(expectedChartData);
  });

  it('should return chartOptions from getMembersPerGenerationChartOptions', () => {
    const expectedChartOptions = { chart: { type: 'bar' } };
    getMembersPerGenerationChartOptionsSpy.mockReturnValue(expectedChartOptions);
    const { state } = useMembersPerGenerationChart(mockProps);
    expect(state.chartOptions.value).toEqual(expectedChartOptions);
  });

  it('should handle undefined membersPerGeneration gracefully', () => {
    const propsWithUndefined = { membersPerGeneration: undefined };
    const { state } = useMembersPerGenerationChart(propsWithUndefined);
    expect(state.chartData.value.series).toEqual([{ name: 'dashboard.membersPerGenerationChart.members', data: [] }]);
    expect(state.chartData.value.categories).toEqual([]);
  });
});