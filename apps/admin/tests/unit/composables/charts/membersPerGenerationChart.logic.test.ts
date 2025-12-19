import { describe, it, expect, vi } from 'vitest';
import { getMembersPerGenerationChartData, getMembersPerGenerationChartOptions } from '@/composables/charts/membersPerGenerationChart.logic';

describe('membersPerGenerationChart.logic', () => {
  const mockT = vi.fn((key) => key); // Simple mock for i18n translation
  const mockTheme = {
    global: {
      current: {
        value: {
          colors: {
            primary: '#007bff',
            'on-surface': '#333',
          },
          dark: false,
        },
      },
    },
  };

  describe('getMembersPerGenerationChartData', () => {
    it('should return empty data when membersPerGeneration is undefined', () => {
      const result = getMembersPerGenerationChartData(undefined, mockT);
      expect(result.series).toEqual([{ name: 'dashboard.membersPerGenerationChart.members', data: [] }]);
      expect(result.categories).toEqual([]);
    });

    it('should return correct data for a single generation', () => {
      const membersPerGeneration = { 1: 5 };
      const result = getMembersPerGenerationChartData(membersPerGeneration, mockT);
      expect(result.series).toEqual([{ name: 'dashboard.membersPerGenerationChart.members', data: [5] }]);
      expect(result.categories).toEqual(['dashboard.membersPerGenerationChart.generation 1']);
    });

    it('should return correct data for multiple generations, sorted', () => {
      const membersPerGeneration = { 3: 10, 1: 5, 2: 7 };
      const result = getMembersPerGenerationChartData(membersPerGeneration, mockT);
      expect(result.series).toEqual([{ name: 'dashboard.membersPerGenerationChart.members', data: [5, 7, 10] }]);
      expect(result.categories).toEqual([
        'dashboard.membersPerGenerationChart.generation 1',
        'dashboard.membersPerGenerationChart.generation 2',
        'dashboard.membersPerGenerationChart.generation 3',
      ]);
    });

    it('should handle empty membersPerGeneration object', () => {
      const membersPerGeneration = {};
      const result = getMembersPerGenerationChartData(membersPerGeneration, mockT);
      expect(result.series).toEqual([{ name: 'dashboard.membersPerGenerationChart.members', data: [] }]);
      expect(result.categories).toEqual([]);
    });
  });

  describe('getMembersPerGenerationChartOptions', () => {
    it('should return correct chart options', () => {
      const chartCategories = ['Generation 1', 'Generation 2'];
      const options = getMembersPerGenerationChartOptions(chartCategories, mockT, mockTheme);

      expect(options.chart?.type).toBe('bar');
      expect(options.xaxis?.categories).toEqual(chartCategories);
      expect(options.xaxis?.title?.text).toBe('dashboard.membersPerGenerationChart.generation');
      expect(options.yaxis?.title?.text).toBe('dashboard.membersPerGenerationChart.totalMembers');
      expect(options.colors).toEqual(['#007bff']);
      expect(options.tooltip?.theme).toBe('light');
      expect(options.tooltip?.y?.formatter?.(10)).toBe('10 dashboard.membersPerGenerationChart.members');
    });

    it('should set dark theme for tooltip when theme is dark', () => {
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
      const chartCategories = ['Generation 1'];
      const options = getMembersPerGenerationChartOptions(chartCategories, mockT, darkTheme);
      expect(options.tooltip?.theme).toBe('dark');
    });
  });
});