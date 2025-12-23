import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useGenderRatioChart } from '@/composables/charts/useGenderRatioChart';

// Mock dependencies directly within vi.mock factories
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(() => ({
    t: vi.fn((key: string) => key),
  })),
}));

vi.mock('vuetify', () => ({
  useTheme: vi.fn(() => ({
    global: {
      current: {
        value: {
          colors: {
            primary: '#1976D2',
            error: '#FF5252',
            'on-surface': '#000000',
          },
          dark: false,
        },
      },
    },
  })),
}));

// Mock the logic functions
vi.mock('@/composables/charts/genderRatioChart.logic', () => ({
  getGenderRatioChartSeries: vi.fn(),
  getGenderRatioChartOptions: vi.fn(),
}));

import { getGenderRatioChartSeries, getGenderRatioChartOptions } from '@/composables/charts/genderRatioChart.logic';

describe('useGenderRatioChart', () => {
  const mockProps = {
    maleRatio: 0.6,
    femaleRatio: 0.4,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    (getGenderRatioChartSeries as Mock).mockImplementation((male: number | undefined, female: number | undefined) => {
      if (male === undefined || female === undefined) return [0, 0];
      return [male * 100, female * 100];
    });
    (getGenderRatioChartOptions as Mock).mockImplementation((t: (key: string) => string, theme: any) => ({
      chart: { type: 'donut' },
      labels: [t('member.gender.male'), t('member.gender.female')],
      colors: [theme.global.current.value.colors.primary, theme.global.current.value.colors.error],
    }));
  });

  it('should call getGenderRatioChartSeries with correct ratios', () => {
    const { state } = useGenderRatioChart(mockProps);
    state.series.value; // Access to trigger computed
    expect(getGenderRatioChartSeries).toHaveBeenCalledWith(mockProps.maleRatio, mockProps.femaleRatio);
  });

  it('should return series from getGenderRatioChartSeries', () => {
    const expectedSeries = [60, 40];
    (getGenderRatioChartSeries as Mock).mockReturnValue(expectedSeries);
    const { state } = useGenderRatioChart(mockProps);
    expect(state.series.value).toEqual(expectedSeries);
  });

  it('should call getGenderRatioChartOptions with correct dependencies', () => {
    const { state } = useGenderRatioChart(mockProps);
    state.chartOptions.value; // Access to trigger computed
    expect(getGenderRatioChartOptions).toHaveBeenCalledWith(expect.any(Function), expect.any(Object));
  });

  it('should return chartOptions from getGenderRatioChartOptions', () => {
    const expectedChartOptions = { chart: { type: 'donut' } };
    (getGenderRatioChartOptions as Mock).mockReturnValue(expectedChartOptions);
    const { state } = useGenderRatioChart(mockProps);
    expect(state.chartOptions.value).toEqual(expectedChartOptions);
  });

  it('should handle undefined ratios gracefully by returning [0, 0] for series', () => {
    const propsWithUndefined = { maleRatio: undefined, femaleRatio: undefined };
    const { state } = useGenderRatioChart(propsWithUndefined);
    expect(state.series.value).toEqual([0, 0]);
  });
});