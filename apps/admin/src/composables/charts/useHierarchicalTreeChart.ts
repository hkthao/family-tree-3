import { ref, onMounted, onUnmounted, watch, nextTick, toRef } from 'vue';
import type { MemberDto, Relationship } from '@/types';
import { transformFamilyData, determineMainChartId } from './hierarchicalTreeChart.logic';
import { createDefaultF3Adapter, type IF3Adapter } from './f3.adapter';

interface UseHierarchicalTreeChartProps {
  familyId: string | null;
  members: MemberDto[];
  relationships: Relationship[];
  rootId: string | null;
}

interface UseHierarchicalTreeChartEmits {
  (event: 'show-member-detail-drawer', memberId: string): void;
}

interface UseHierarchicalTreeChartDeps {
  f3Adapter: IF3Adapter;
  t: (key: string) => string; // The translation function
}

export function useHierarchicalTreeChart(
  props: UseHierarchicalTreeChartProps,
  emit: UseHierarchicalTreeChartEmits,
  deps?: Partial<UseHierarchicalTreeChartDeps>
) {
  const chartContainer = ref<HTMLDivElement | null>(null);
  let chartInstance: any = null; // To hold the chart instance

  // Default dependencies
  const defaultDeps: UseHierarchicalTreeChartDeps = {
    f3Adapter: createDefaultF3Adapter(emit),
    t: (key: string) => key, // Placeholder, should be injected via useI18n().t in actual usage
  };
  const { f3Adapter, t } = { ...defaultDeps, ...deps };


  const renderChart = async (currentMembers: MemberDto[]) => {
    if (!chartContainer.value) {
      return;
    }

    f3Adapter.clearChart(chartContainer.value); // Clear existing chart

    const { filteredMembers, transformedData } = transformFamilyData(
      currentMembers,
      props.relationships,
      props.rootId
    );

    if (transformedData.length === 0) {
      chartContainer.value.innerHTML = `<div class="empty-message">${t('familyTree.noMembersMessage')}</div>`;
      chartInstance = null;
      return;
    }

    // Wrap the chart creation and update in nextTick
    await nextTick();

    chartInstance = f3Adapter.createChart(chartContainer.value, transformedData);

    const mainIdToSet = determineMainChartId(filteredMembers, transformedData, props.rootId);

    if (mainIdToSet) {
      f3Adapter.updateChart(chartInstance, mainIdToSet, { initial: true });
    } else {
      console.warn('No main ID could be determined for the family tree chart.');
    }
  };

  const updateChartMainId = (newMainId: string) => {
    if (chartInstance && newMainId) {
      f3Adapter.updateChart(chartInstance, newMainId, { initial: true });
    }
  };

  onMounted(() => {
    if (props.familyId) {
      renderChart(props.members);
    }
  });

  onUnmounted(() => {
    if (chartInstance && chartContainer.value) {
      f3Adapter.clearChart(chartContainer.value);
      chartInstance = null;
    }
  });

  watch(
    [toRef(props, 'familyId'), toRef(props, 'members'), toRef(props, 'relationships'), toRef(props, 'rootId')],
    () => {
      if (props.familyId) {
        renderChart(props.members);
      } else {
        renderChart([]);
      }
    },
    { deep: true, immediate: true }
  );

  return {
    chartContainer,
    actions: {
      renderChart, // Expose renderChart if needed for external triggers
      updateChartMainId,
    }
  };
}
