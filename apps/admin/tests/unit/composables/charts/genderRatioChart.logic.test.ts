import { describe, it, expect, vi } from 'vitest';
import { getGenderRatioChartSeries, getGenderRatioChartOptions } from '@/composables/charts/genderRatioChart.logic';

describe('genderRatioChart.logic', () => {
  describe('getGenderRatioChartSeries', () => {
    it('should return correct series when maleRatio and femaleRatio are defined', () => {
      const maleRatio = 0.6;
      const femaleRatio = 0.4;
      expect(getGenderRatioChartSeries(maleRatio, femaleRatio)).toEqual([60, 40]);
    });

    it('should return [0, 0] when maleRatio is undefined', () => {
      const maleRatio = undefined;
      const femaleRatio = 0.4;
      expect(getGenderRatioChartSeries(maleRatio, femaleRatio)).toEqual([0, 0]);
    });

    it('should return [0, 0] when femaleRatio is undefined', () => {
      const maleRatio = 0.6;
      const femaleRatio = undefined;
      expect(getGenderRatioChartSeries(maleRatio, femaleRatio)).toEqual([0, 0]);
    });

    it('should return [0, 0] when both ratios are undefined', () => {
      const maleRatio = undefined;
      const femaleRatio = undefined;
      expect(getGenderRatioChartSeries(maleRatio, femaleRatio)).toEqual([0, 0]);
    });

    it('should handle zero ratios correctly', () => {
      const maleRatio = 0;
      const femaleRatio = 0;
      expect(getGenderRatioChartSeries(maleRatio, femaleRatio)).toEqual([0, 0]);
    });

    it('should handle small decimal ratios correctly', () => {
      const maleRatio = 0.005;
      const femaleRatio = 0.995;
      expect(getGenderRatioChartSeries(maleRatio, femaleRatio)).toEqual([0.5, 99.5]);
    });
  });

  describe('getGenderRatioChartOptions', () => {
    const mockT = vi.fn((key) => key); // Simple mock for i18n translation
    const mockTheme = {
      global: {
        current: {
          value: {
            colors: {
              primary: '#007bff',
              error: '#dc3545',
              'on-surface': '#333',
            },
            dark: false,
          },
        },
      },
    };

    it('should return correct chart options', () => {
      const options = getGenderRatioChartOptions(mockT, mockTheme);

      expect(options.chart?.type).toBe('donut');
      expect(options.labels).toEqual(['member.gender.male', 'member.gender.female']);
      expect(options.colors).toEqual(['#007bff', '#dc3545']);
      expect(options.dataLabels?.enabled).toBe(true);
      expect(options.dataLabels?.formatter?.(50)).toBe('50.0%');
      expect(options.dataLabels?.style?.colors).toEqual([mockTheme.global.current.value.colors['on-surface']]);
      expect(options.legend?.labels?.colors).toEqual([mockTheme.global.current.value.colors['on-surface']]);
      expect(options.plotOptions?.pie?.donut?.labels?.total?.label).toBe('dashboard.genderRatioChart.total');
      // Test the formatter for total as well
      const mockGlobals = {
        globals: {
          seriesTotals: [60, 40],
        },
      };
      expect(options.plotOptions?.pie?.donut?.labels?.total?.formatter?.(mockGlobals)).toBe('100.0%');
    });

    it('should set dark theme colors when theme is dark', () => {
      const darkTheme = {
        ...mockTheme,
        global: {
          current: {
            value: {
              ...mockTheme.global.current.value,
              dark: true,
            },
          },
        },
      };
      const options = getGenderRatioChartOptions(mockT, darkTheme);
      expect(options.plotOptions?.pie?.donut?.labels?.total?.color).toBe(darkTheme.global.current.value.colors['on-surface']);
    });
  });
});