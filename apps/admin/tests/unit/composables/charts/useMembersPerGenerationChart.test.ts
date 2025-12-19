import { describe, it, expect, vi, beforeEach } from 'vitest';
import { computed, ref } from 'vue';
import { useMembersPerGenerationChart } from '@/composables/charts/useMembersPerGenerationChart';
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

describe('useMembersPerGenerationChart', () => {
  const mockT = vi.fn((key: string) => key);
  const mockTheme = {
    global: {
      current: ref({
        colors: {
          primary: '#1976D2',
          'on-surface': '#000000',
        },
        dark: false, // For tooltip theme testing
      }),
    },
  };

  beforeEach(() => {
    vi.clearAllMocks();
    (useI18n as vi.Mock).mockReturnValue({ t: mockT });
    (useTheme as vi.Mock).mockReturnValue(mockTheme);
  });

  // Test case 1: chartData calculation with valid data
  it('should correctly calculate chartData with valid members per generation', () => {
    const membersPerGeneration = { 1: 5, 3: 10, 2: 7 }; // Unsorted generations
    const props = { membersPerGeneration };
    const { chartData } = useMembersPerGenerationChart(props);

    // Expect generations to be sorted
    expect(chartData.value.categories).toEqual([
      'dashboard.membersPerGenerationChart.generation 1',
      'dashboard.membersPerGenerationChart.generation 2',
      'dashboard.membersPerGenerationChart.generation 3',
    ]);
    expect(mockT).toHaveBeenCalledWith('dashboard.membersPerGenerationChart.generation');
    expect(chartData.value.series).toEqual([
      { name: 'dashboard.membersPerGenerationChart.members', data: [5, 7, 10] },
    ]);
    expect(mockT).toHaveBeenCalledWith('dashboard.membersPerGenerationChart.members');
  });

  // Test case 2: chartData calculation with empty data
  it('should return empty series and categories if members per generation is undefined', () => {
    const props = { membersPerGeneration: undefined };
    const { chartData } = useMembersPerGenerationChart(props);

    expect(chartData.value.categories).toEqual([]);
    expect(chartData.value.series).toEqual([{ name: 'dashboard.membersPerGenerationChart.members', data: [] }]);
  });

  // Test case 3: chartOptions dynamic values (labels, colors, formatter)
  it('should return correct chartOptions with dynamic labels and colors', () => {
    const membersPerGeneration = { 1: 5, 2: 7 };
    const props = { membersPerGeneration };
    const { chartOptions } = useMembersPerGenerationChart(props);

    // Assert colors
    expect(chartOptions.value.colors).toEqual([mockTheme.global.current.value.colors.primary]);
    expect(chartOptions.value.chart?.foreColor).toBe(mockTheme.global.current.value.colors['on-surface']);

    // Assert x-axis title and labels color
    expect(chartOptions.value.xaxis?.title?.text).toBe('dashboard.membersPerGenerationChart.generation');
    expect(chartOptions.value.xaxis?.title?.style?.color).toBe(mockTheme.global.current.value.colors['on-surface']);
    expect(chartOptions.value.xaxis?.labels?.style?.colors).toBe(mockTheme.global.current.value.colors['on-surface']);

    // Assert y-axis title and labels color
    expect(chartOptions.value.yaxis?.title?.text).toBe('dashboard.membersPerGenerationChart.totalMembers');
    expect(chartOptions.value.yaxis?.title?.style?.color).toBe(mockTheme.global.current.value.colors['on-surface']);
    expect(chartOptions.value.yaxis?.labels?.style?.colors).toBe(mockTheme.global.current.value.colors['on-surface']);

    // Assert tooltip formatter
    const tooltipFormatter = chartOptions.value.tooltip?.y?.formatter as Function;
    expect(tooltipFormatter(10)).toBe('10 dashboard.membersPerGenerationChart.members');
    expect(mockT).toHaveBeenCalledWith('dashboard.membersPerGenerationChart.members');

    // Assert tooltip theme
    expect(chartOptions.value.tooltip?.theme).toBe('light');
  });

  // Test case 4: Tooltip theme in dark mode
  it('should set tooltip theme to dark if theme is dark', () => {
    mockTheme.global.current.value.dark = true; // Set theme to dark
    const membersPerGeneration = { 1: 5 };
    const props = { membersPerGeneration };
    const { chartOptions } = useMembersPerGenerationChart(props);
    expect(chartOptions.value.tooltip?.theme).toBe('dark');
  });
});
