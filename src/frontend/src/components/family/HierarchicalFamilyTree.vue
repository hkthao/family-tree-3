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
import { useHierarchicalTreeChart } from '@/composables/useHierarchicalTreeChart';
import type { Member, Relationship } from '@/types';

const { t } = useI18n();
const emit = defineEmits([
  'show-member-detail-drawer', // New emit event
]);

const props = defineProps({
  familyId: { type: String, default: null },
  members: { type: Array<Member>, default: () => [] },
  relationships: { type: Array<Relationship>, default: () => [] },
  rootId: { type: String, default: null }, // New prop for specifying the root member ID
});

const { chartContainer } = useHierarchicalTreeChart(
  props,
  emit,
  t
);
</script>

<style>
.hierarchical-tree-container {
  position: relative;
  width: 100%;
}

.main_svg {
  width: 100% !important;
  min-height: 60vh;
  height: 100% !important;
}

.main_svg path.link {
  stroke: rgb(var(--v-theme-on-surface));
}

.f3 {
  cursor: pointer;
  height: 100%;
}

.f3 div.card {
  cursor: pointer;
  pointer-events: auto;
  color: rgb(var(--v-theme-on-surface));
  position: relative;
  margin-top: -30px;
  margin-left: -30px;
}

.card-menu-button {
  position: absolute;
  top: 0px;
  left: -10px;
  cursor: pointer;
  z-index: 10;
  font-size: 18px;
}

.card-menu-button:hover {
  color: rgb(var(--v-theme-primary)) !important;
}

.f3 div.card-image {
  border-radius: 50%;
  padding: 5px;
  width: 60px;
  height: 60px;
}

.f3 div.card-image div.card-label,
.f3 div.card-image div.card-dates {
  position: absolute;
  left: 50%;
  transform: translateX(-50%) !important;
  max-width: 150%;
  text-align: center;
  background-color: rgba(var(--v-theme-surface-variant-rgb), 0.7);
  color: rgb(var(--v-theme-on-surface));
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  border-radius: 3px;
  padding: 1px 6px;
  font-size: 12px;
}

.f3 div.card-image div.card-dates {
  bottom: -35px;
  ;
}

.f3 div.card-image div.card-label {
  bottom: -20px;
}

.f3 div.card-image img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
}

.f3 div.card-text {
  padding: 5px;
  border-radius: 3px;
  width: 120px;
  height: 70px;
  overflow: hidden;
  line-height: 1.2;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  text-align: center;
}

.f3 div.card-text .card-label {
  font-weight: bold;
  margin-bottom: 4px;
}

.f3 div.card>div {
  transition: transform 0.2s ease-in-out;
}

.f3 div.card:hover>div {
  transform: scale(1.1);
}

.f3 div.card-main>div {
  transform: scale(1.2) !important;
}

.f3 div.card-female {
  background-color: rgb(var(--v-theme-secondary));
}

.f3 div.card-male {
  background-color: rgb(var(--v-theme-primary));
}

.f3 div.card-genderless {
  background-color: lightgray;
}

.f3 div.card-main {
  box-shadow: 0 0 20px 0 rgba(0, 0, 0, 0.8);
}

.empty-message {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  flex-direction: column;
  height: 100%;
}

/* Legend Styles */
.legend {
  position: absolute;
  top: 10px;
  left: 0px;
  background: rgba(var(--v-theme-surface-variant-rgb), 0.8);
  padding: 10px;
  border-radius: 5px;
  color: rgb(var(--v-theme-on-surface));
}

.legend-item {
  display: flex;
  align-items: center;
  margin-bottom: 5px;
}

.legend-item:last-child {
  margin-bottom: 0;
}

.legend-color-box {
  width: 15px;
  height: 15px;
  margin-right: 8px;
  border-radius: 3px;
  border: 1px solid #ccc;
}

.legend-male {
  background-color: rgb(var(--v-theme-primary));
}

.legend-female {
  background-color: rgb(var(--v-theme-secondary));
}
</style>