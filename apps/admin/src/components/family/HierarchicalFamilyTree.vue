<template>
  <div class="hierarchical-tree-container">
    <div ref="chartContainer" class="f3 flex-grow-1" data-testid="family-tree-canvas"></div>
    <div class="legend">
      <div class="legend-item">
        <span class="legend-color-box legend-male"></span>
        <span>{{ t('member.gender.male') }}</span>
      </div>
      <div class="legend-item">
        <span class="legend-color-box legend-female"></span>
        <span>{{ t('member.gender.female') }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useHierarchicalTreeChart } from '@/composables';
import type { MemberDto, Relationship } from '@/types';

const { t } = useI18n();
const emit = defineEmits([
  'show-member-detail-drawer', // New emit event
]);

const props = defineProps({
  familyId: { type: String, default: null },
  members: { type: Array<MemberDto>, default: () => [] },
  relationships: { type: Array<Relationship>, default: () => [] },
  rootId: { type: String, default: null }, // New prop for specifying the root member ID
});

const { chartContainer } = useHierarchicalTreeChart(
  props,
  emit,
  { t }
);
</script>

<style>
@import '@/styles/family-tree-chart.css';
</style>