<template>
  <v-container fluid class="pa-0">
    <v-card>
      <v-tabs v-model="tab" align-tabs="center" color="primary">
        <v-tab value="hierarchical">{{ t('familyTree.tab.hierarchical') }}</v-tab>
        <v-tab value="force-directed">{{ t('familyTree.tab.forceDirected') }}</v-tab>
      </v-tabs>

      <v-card-text class="pa-0">
        <v-window v-model="tab">
          <v-window-item value="hierarchical">
            <HierarchicalFamilyTree
              v-if="familyId && members.length > 0"
              :family-id="familyId"
              :members="members"
              :relationships="relationships"
              :root-id="rootId"
              @update:rootId="(newRootId: string) => rootId = newRootId"
              :onNodeClick="handleNodeClick"
            />
            <v-alert v-else type="info" prominent>{{ t('familyTree.noDataMessage') }}</v-alert>
          </v-window-item>

          <v-window-item value="force-directed">
            <ForceDirectedFamilyTree
              v-if="familyId && members.length > 0"
              :family-id="familyId"
              :members="members"
              :isMobile="true"
              :relationships="relationships"
              :root-id="rootId"
              @update:rootId="(newRootId: string) => rootId = newRootId"
              :onNodeClick="handleNodeClick"
            />
            <v-alert v-else type="info" prominent>{{ t('familyTree.noDataMessage') }}</v-alert>
          </v-window-item>
        </v-window>
      </v-card-text>
    </v-card>
    <MemberActionDialog
      v-model="isActionDialogOpen"
      :member-id="selectedMemberIdForAction"
      :member-name="selectedMemberNameForAction"
      @view-details="handleViewDetails"
      @view-relationships="handleViewRelationships"
    />
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import HierarchicalFamilyTree from '@/components/family/HierarchicalFamilyTree.vue';
import ForceDirectedFamilyTree from '@/components/family/ForceDirectedFamilyTree.vue';
import MemberActionDialog from '@/components/member/MemberActionDialog.vue'; // Import MemberActionDialog
import type { MemberDto, Relationship } from '@/types';

interface WindowFamilyTreeData {
  familyId?: string;
  members?: MemberDto[];
  relationships?: Relationship[];
  rootId?: string; // Add rootId here
}

const { t } = useI18n();

const tab = ref('hierarchical');
const familyId = ref<string | undefined>(undefined);
const members = ref<MemberDto[]>([]);
const relationships = ref<Relationship[]>([]);
const rootId = ref<string | undefined>(undefined); // Declare rootId ref

// State for MemberActionDialog
const isActionDialogOpen = ref(false);
const selectedMemberIdForAction = ref('');
const selectedMemberNameForAction = ref('');

const handleNodeClick = (memberId: string, memberName: string) => {
  selectedMemberIdForAction.value = memberId;
  selectedMemberNameForAction.value = memberName;
  isActionDialogOpen.value = true;
};

const handleViewDetails = (memberId: string) => {
  // In a mobile view, this might navigate to a details page or open a different drawer
  // For now, let's just log and close the action dialog
  console.log(`View Details for member: ${memberId}`);
  isActionDialogOpen.value = false;
  // TODO: Implement actual navigation or drawer for member details in mobile view
};

const handleViewRelationships = (memberId: string) => {
  rootId.value = memberId;
  isActionDialogOpen.value = false;
};

onMounted(() => {
  if (window.familyTreeData) {
    const data: WindowFamilyTreeData = window.familyTreeData;
    familyId.value = data.familyId;
    members.value = data.members || [];
    relationships.value = data.relationships || [];
    rootId.value = data.rootId; // Extract rootId
  }
});
</script>

<style scoped>
/* Add any specific styles for this view here */
</style>
