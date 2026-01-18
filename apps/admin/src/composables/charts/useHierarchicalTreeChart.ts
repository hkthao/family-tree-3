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
  (event: 'update:rootId', memberId: string): void;
}

interface UseHierarchicalTreeChartDeps {
  f3Adapter: IF3Adapter;
  t: (key: string, values?: Record<string, unknown>) => string; // The translation function, updated to support interpolation
}

export const RENDER_SAFE_THRESHOLD = 100; // Define a safe threshold for rendering nodes

export function useHierarchicalTreeChart(
  props: UseHierarchicalTreeChartProps,
  emit: UseHierarchicalTreeChartEmits,
  deps: Partial<UseHierarchicalTreeChartDeps>,
  onNodeClick: (memberId: string, memberName: string) => void
) {
  const chartContainer = ref<HTMLDivElement | null>(null);
  let chartInstance: any = null; // To hold the chart instance

  // Default dependencies
  const defaultDeps: UseHierarchicalTreeChartDeps = {
    f3Adapter: createDefaultF3Adapter(emit, onNodeClick),
    t: (key: string) => key, // Placeholder, should be injected via useI18n().t in actual usage
  };
  const { f3Adapter, t } = { ...defaultDeps, ...deps };


  const renderChart = async (currentMembers: MemberDto[], currentRelationships: Relationship[]) => { // Updated parameters
    if (!chartContainer.value) {
      return;
    }

    f3Adapter.clearChart(chartContainer.value); // Clear existing chart

    const { members, transformedData } = transformFamilyData( // Changed filteredMembers to members
      currentMembers,
      currentRelationships,
      props.rootId
    );

    if (transformedData.length === 0) {
      chartContainer.value.innerHTML = `<div class="empty-message">${t('familyTree.noMembersMessage')}</div>`;
      chartInstance = null;
      return;
    }
    
    // Safety check: Prevent rendering if data size is unexpectedly large
    if (members.length > RENDER_SAFE_THRESHOLD) {
      console.error(`Attempted to render ${members.length} nodes, exceeding safe threshold of ${RENDER_SAFE_THRESHOLD}. Preventing render.`);
      chartContainer.value.innerHTML = `<div class="empty-message error-message">${t('familyTree.renderingLimitExceeded', { limit: RENDER_SAFE_THRESHOLD })}</div>`;
      chartInstance = null;
      return;
    }

    // Wrap the chart creation and update in nextTick
    await nextTick();

    console.log(`Rendering Hierarchical Family Tree with ${transformedData.length} nodes.`); // Simple log

    chartInstance = f3Adapter.createChart(chartContainer.value, transformedData);

    const mainIdToSet = determineMainChartId(members, transformedData, props.rootId); // Changed filteredMembers to members

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
      renderChart(props.members, props.relationships);
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
    ([newFamilyId, newMembers, newRelationships]) => {
      if (newFamilyId) {
        renderChart(newMembers, newRelationships);
      } else {
        renderChart([], []);
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
