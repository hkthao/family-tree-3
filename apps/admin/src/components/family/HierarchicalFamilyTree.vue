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
    <MemberActionDialog
      v-model="isActionDialogOpen"
      :member-id="selectedMemberId"
      :member-name="selectedMemberName"
      @view-details="handleViewDetails"
      @view-relationships="handleViewRelationships"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useHierarchicalTreeChart } from '@/composables';
import type { MemberDto, Relationship } from '@/types';
import MemberActionDialog from '@/components/member/MemberActionDialog.vue';

const { t } = useI18n();
const emit = defineEmits([
  'show-member-detail-drawer', // New emit event
  'update:rootId', // New emit event to update rootId
]);

const props = defineProps({
  familyId: { type: String, default: null },
  members: { type: Array<MemberDto>, default: () => [] },
  relationships: { type: Array<Relationship>, default: () => [] },
  rootId: { type: String, default: null }, // New prop for specifying the root member ID
});

const isActionDialogOpen = ref(false);
const selectedMemberId = ref('');
const selectedMemberName = ref('');

const onNodeClick = (memberId: string, memberName: string) => {
  selectedMemberId.value = memberId;
  selectedMemberName.value = memberName;
  isActionDialogOpen.value = true;
};

const { chartContainer } = useHierarchicalTreeChart(
  props,
  emit,
  { t },
  onNodeClick
);

const handleViewDetails = (memberId: string) => {
  emit('show-member-detail-drawer', memberId);
};

const handleViewRelationships = (memberId: string) => {
  emit('update:rootId', memberId);
};
</script>

<style>
@import '@/styles/family-tree-chart.css';
</style>