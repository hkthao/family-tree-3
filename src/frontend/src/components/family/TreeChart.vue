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
    <v-tooltip :text="t('member.add')" v-if="canAddMember">
      <template v-slot:activator="{ props }">
        <v-btn icon v-bind="props" color="primary" @click="handleAddMember" class="mr-2">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </template>
    </v-tooltip>
    <!-- <v-text-field v-model="treeVisualizationStore.searchQuery" :label="t('family.tree.searchMember')"
      prepend-inner-icon="mdi-magnify" single-line hide-details clearable class="mr-4"></v-text-field> -->
    <MemberAutocomplete v-model="selectedRootMemberId" :label="t('family.tree.filterByRootMember')"
      :family-id="props.familyId" clearable hide-details hide-chips class="mr-4" style="max-width: 250px;" />
  </v-toolbar>
  <HierarchicalFamilyTree v-if="chartMode === 'hierarchical'" :family-id="props.familyId" :members="members"
    :relationships="relationships" :root-id="selectedRootMemberId ?? undefined"
    @show-member-detail-drawer="handleShowMemberDetailDrawer" :read-only="props.readOnly" />
  <ForceDirectedFamilyTree v-else :family-id="props.familyId" :members="members" :relationships="relationships"
    @show-member-detail-drawer="handleShowMemberDetailDrawer" @edit-member="handleEditMember"
    :read-only="props.readOnly" />

  <v-navigation-drawer v-model="addMemberDrawer" location="right" temporary width="650" v-if="canAddMember">
    <MemberAddView v-if="addMemberDrawer" :family-id="props.familyId"
      :initial-relationship-data="initialRelationshipData" @close="addMemberDrawer = false"
      @saved="handleMemberAdded" />
  </v-navigation-drawer>

  <!-- New v-navigation-drawer for member details -->
  <v-navigation-drawer v-model="memberDetailDrawer" location="right" temporary width="650">
    <MemberDetailView v-if="memberDetailDrawer && selectedMemberId" :member-id="selectedMemberId"
      @close="memberDetailDrawer = false" @member-deleted="handleMemberDeleted"
      @add-member-with-relationship="handleAddMemberWithRelationship" @edit-member="handleEditMember"
      :read-only="props.readOnly" />
  </v-navigation-drawer>

  <!-- New v-navigation-drawer for member edit -->
  <v-navigation-drawer v-model="editMemberDrawer" location="right" temporary width="650" v-if="canEditMember">
    <MemberEditView v-if="editMemberDrawer && selectedMemberId" :member-id="selectedMemberId"
      @close="editMemberDrawer = false" @saved="handleMemberEdited" />
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { HierarchicalFamilyTree, ForceDirectedFamilyTree } from '@/components/family';
import { useTreeVisualizationStore } from '@/stores/tree-visualization.store';
import MemberAddView from '@/views/member/MemberAddView.vue';
import MemberDetailView from '@/views/member/MemberDetailView.vue';
import MemberEditView from '@/views/member/MemberEditView.vue';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue'; // Import MemberAutocomplete
import { useAuth } from '@/composables/useAuth';

const props = defineProps({
  familyId: { type: String, default: null },
  initialMemberId: { type: String, default: null }, // New prop for initial member ID
  readOnly: { type: Boolean, default: false },
});
const { t } = useI18n();
const chartMode = ref('hierarchical'); // Default view
const treeVisualizationStore = useTreeVisualizationStore();
const { isAdmin, isFamilyManager } = useAuth();

const canAddMember = computed(() => {
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const canEditMember = computed(() => {
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const addMemberDrawer = ref(false); // Control visibility of the add member drawer
const selectedMemberId = ref<string | null>(null); // New ref for selected member ID
const memberDetailDrawer = ref(false); // New ref for member detail drawer visibility
const editMemberDrawer = ref(false); // New ref for member edit drawer visibility
const initialRelationshipData = ref<any | null>(null); // New ref for initial relationship data
const selectedRootMemberId = ref<string | null>(null); // New ref for selected root member ID

// New computed properties for members and relationships from the store
const members = computed(() => treeVisualizationStore.getFilteredMembers(props.familyId));
const relationships = computed(() => treeVisualizationStore.getFilteredRelationships(props.familyId));

// New initialize function to fetch data
const initialize = async (familyId: string, initialMemberId: string | null) => {
  if (familyId) {
    await treeVisualizationStore.fetchTreeData(familyId, initialMemberId || undefined);
  }
};

const handleAddMember = () => {
  initialRelationshipData.value = null; // Clear any previous relationship data
  addMemberDrawer.value = true;
};
const handleMemberAdded = () => {
  addMemberDrawer.value = false;
  // Refresh tree data after a new member is added
  if (props.familyId) {
    treeVisualizationStore.fetchTreeData(props.familyId);
  }
};

// New handler for showing member detail drawer
const handleShowMemberDetailDrawer = (memberId: string) => {
  if (canEditMember.value) {
    selectedMemberId.value = memberId;
    memberDetailDrawer.value = true;
  }
};

const handleMemberDeleted = () => {
  memberDetailDrawer.value = false;
  if (props.familyId) {
    treeVisualizationStore.fetchTreeData(props.familyId);
  }
};

const handleAddMemberWithRelationship = (data: any) => {
  initialRelationshipData.value = data;
  memberDetailDrawer.value = false;
  addMemberDrawer.value = true;
};

// New handler for editing a member
const handleEditMember = (memberId: string) => {
  if (canEditMember.value) {
    selectedMemberId.value = memberId;
    memberDetailDrawer.value = false; // Close detail drawer
    editMemberDrawer.value = true; // Open edit drawer
  }
};

const handleMemberEdited = () => {
  editMemberDrawer.value = false;
  if (props.familyId) {
    treeVisualizationStore.fetchTreeData(props.familyId);
  }
};

// Call initialize on mounted
onMounted(async () => {
  if (props.familyId) {
    await initialize(props.familyId, props.initialMemberId);
  }
});

// Watch for familyId and initialMemberId changes and re-initialize
watch([() => props.familyId, () => props.initialMemberId], async ([newFamilyId, newInitialMemberId]) => {
  if (newFamilyId) {
    await initialize(newFamilyId, newInitialMemberId);
  }
});    </script>
