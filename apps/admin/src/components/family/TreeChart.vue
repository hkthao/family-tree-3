<template>
  <v-toolbar class="mb-1">
    <v-btn-toggle color="primary" class="ms-4" v-model="chartMode" mandatory>
      <v-btn value="hierarchical">
        <v-icon start>mdi-file-tree</v-icon>
        {{ t('family.tree.view.hierarchical') }}
      </v-btn>
      <v-btn value="force-directed">
        <v-icon start>mdi-share-variant</v-icon>
        {{ t('family.tree.view.network') }}
      </v-btn>
    </v-btn-toggle>
    <v-spacer></v-spacer>
    <v-tooltip :text="t('common.refresh')">
      <template v-slot:activator="{ props }">
        <v-btn icon v-bind="props" color="primary" @click="handleRefresh" class="mr-2">
          <v-icon>mdi-refresh</v-icon>
        </v-btn>
      </template>
    </v-tooltip>
    <v-tooltip :text="t('member.add')" v-if="canAddMember">
      <template v-slot:activator="{ props }">
        <v-btn icon v-bind="props" color="primary" @click="handleAddMember" class="mr-2">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </template>
    </v-tooltip>
    <MemberAutocomplete v-model="selectedRootMemberId" :label="t('family.tree.filterByRootMember')"
      :family-id="props.familyId" clearable hide-details hide-chips class="mr-4" style="max-width: 250px;" />
  </v-toolbar>
  <HierarchicalFamilyTree v-if="chartMode === 'hierarchical'" :family-id="props.familyId" :members="members"
    :relationships="relationships" :root-id="selectedRootMemberId ?? undefined"
    @show-member-detail-drawer="handleShowMemberDetailDrawer" :read-only="props.readOnly"
    @update:rootId="(newRootId: string) => selectedRootMemberId = newRootId" :onNodeClick="handleNodeClick" />
  <ForceDirectedFamilyTree v-else :family-id="props.familyId" :members="members" :relationships="relationships"
    @show-member-detail-drawer="handleShowMemberDetailDrawer" @edit-member="handleEditMember"
    :read-only="props.readOnly" :root-id="selectedRootMemberId ?? undefined"
    @update:rootId="(newRootId: string) => selectedRootMemberId = newRootId" :onNodeClick="handleNodeClick" />

  <v-navigation-drawer v-model="addMemberDrawer" location="right" temporary width="650" v-if="canAddMember">
    <MemberAddView v-if="addMemberDrawer" :family-id="props.familyId"
      :initial-relationship-data="initialRelationshipData" @close="addMemberDrawer = false"
      @saved="handleMemberAdded" />
  </v-navigation-drawer>

  <!-- New v-navigation-drawer for member details -->
  <v-navigation-drawer v-model="memberDetailDrawer" location="right" temporary width="650">
    <MemberDetailTabsView v-if="memberDetailDrawer && selectedMemberId" :member-id="selectedMemberId"
      @close="memberDetailDrawer = false" @member-deleted="handleMemberDeleted"
      @add-member-with-relationship="handleAddMemberWithRelationship" @edit-member="handleEditMember"
      :read-only="props.readOnly" />
  </v-navigation-drawer>

  <!-- New v-navigation-drawer for member edit -->
  <v-navigation-drawer v-model="editMemberDrawer" location="right" temporary width="650" v-if="canEditMember">
    <MemberEditView v-if="editMemberDrawer && selectedMemberId" :member-id="selectedMemberId"
      @close="editMemberDrawer = false" @saved="handleMemberEdited" />
  </v-navigation-drawer>

  <MemberActionDialog
    v-model="isActionDialogOpen"
    :member-id="selectedMemberIdForAction"
    :member-name="selectedMemberNameForAction"
    @view-details="handleViewDetails"
    @view-relationships="handleViewRelationships"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, computed, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { HierarchicalFamilyTree, ForceDirectedFamilyTree } from '@/components/family';
import MemberAddView from '@/views/member/MemberAddView.vue';
import MemberDetailTabsView from '@/views/member/MemberDetailTabsView.vue';
import MemberEditView from '@/views/member/MemberEditView.vue';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useAuth } from '@/composables';
import { useTreeVisualization } from '@/composables/family/useTreeVisualization'; // Import the new composable
import MemberActionDialog from '@/components/member/MemberActionDialog.vue'; // Import MemberActionDialog

const props = defineProps({
  familyId: { type: String, default: null },
  initialMemberId: { type: String, default: null },
  readOnly: { type: Boolean, default: false },
});
const { t } = useI18n();
const chartMode = ref('hierarchical');

const {
  state: { members, relationships, selectedRootMemberId },
  actions: { fetchTreeData },
} = useTreeVisualization(toRef(props, 'familyId'), props.initialMemberId);

const { state } = useAuth();

const canAddMember = computed(() => {
  return !props.readOnly && (state.isAdmin.value || state.isFamilyManager.value(props.familyId));
});

const canEditMember = computed(() => {
  return !props.readOnly && (state.isAdmin.value || state.isFamilyManager.value(props.familyId));
});

const addMemberDrawer = ref(false);
const selectedMemberId = ref<string | null>(null);
const memberDetailDrawer = ref(false);
const editMemberDrawer = ref(false);
const initialRelationshipData = ref<any | null>(null);

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
  handleShowMemberDetailDrawer(memberId);
  isActionDialogOpen.value = false;
};

const handleViewRelationships = (memberId: string) => {
  selectedRootMemberId.value = memberId;
  isActionDialogOpen.value = false;
};

const handleAddMember = () => {
  initialRelationshipData.value = null;
  addMemberDrawer.value = true;
};
const handleMemberAdded = () => {
  addMemberDrawer.value = false;
  if (props.familyId) {
    fetchTreeData(props.familyId, selectedRootMemberId.value ?? undefined); // Pass selectedRootMemberId to refetch correctly
  }
};

const handleShowMemberDetailDrawer = (memberId: string) => {
  if (canEditMember.value) {
    selectedMemberId.value = memberId;
    memberDetailDrawer.value = true;
  }
};

const handleMemberDeleted = () => {
  memberDetailDrawer.value = false;
  if (props.familyId) {
    fetchTreeData(props.familyId, selectedRootMemberId.value ?? undefined); // Pass selectedRootMemberId to refetch correctly
  }
};

const handleAddMemberWithRelationship = (data: any) => {
  initialRelationshipData.value = data;
  memberDetailDrawer.value = false;
  addMemberDrawer.value = true;
};

const handleEditMember = (memberId: string) => {
  if (canEditMember.value) {
    selectedMemberId.value = memberId;
    memberDetailDrawer.value = false;
    editMemberDrawer.value = true;
  }
};

const handleMemberEdited = () => {
  editMemberDrawer.value = false;
  if (props.familyId) {
    fetchTreeData(props.familyId, selectedRootMemberId.value ?? undefined); // Pass selectedRootMemberId to refetch correctly
  }
};

const handleRefresh = () => {
  if (props.familyId) {
    fetchTreeData(props.familyId, selectedRootMemberId.value ?? undefined); // Pass selectedRootMemberId to refetch correctly
  }
};

// No need for onMounted and watch here, useTreeVisualization handles it internally
// However, ensure fetchTreeData is called initially
onMounted(() => {
  if (props.familyId) {
    fetchTreeData(props.familyId, props.initialMemberId ?? undefined);
  }
});

// Remove the watch below as it's now handled internally by useTreeVisualization's watch effects.
// watch([() => props.familyId, () => props.initialMemberId], ([newFamilyId, newInitialMemberId]) => {
//   if (newFamilyId) {
//     fetchTreeData(newFamilyId, newInitialMemberId ?? undefined);
//   }
// });
</script>
