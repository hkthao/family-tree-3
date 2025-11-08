<template>
  <v-card data-testid="family-tree-view">
    <v-card-title>
      <div class="d-flex flex-row">
        <h1 class="text-h5 flex-grow-1">
          {{ t('family.tree.title') }}
        </h1>
        <family-auto-complete v-model="selectedFamilyId" class="tree-filter-input"
          :label="t('family.tree.filterByFamily')" clearable @update:modelValue="onFamilyChange" hideDetails />
      </div>
    </v-card-title>

    <v-card-text>
      <div v-if="!selectedFamilyId" class="text-center pa-8">
        <p>{{ t('event.messages.selectFamily') }}</p>
      </div>
      <template v-else>
        <TreeChart
          :family-id="selectedFamilyId"
          @add-member="openAddMemberDrawer"
          @edit-member="openEditMemberDrawer"
          @delete-member="confirmDeleteMember"
          @add-father="openAddFatherDrawer"
          @add-mother="openAddMotherDrawer"
          @add-child="openAddChildDrawer"
        />
      </template>
    </v-card-text>

    <!-- Member Add Drawer -->
    <v-navigation-drawer v-model="addMemberDrawer" location="right" temporary width="650">
      <MemberAddView
        v-if="addMemberDrawer"
        :family-id="selectedFamilyId"
        :parent-id="parentMemberId"
        :relationship-type="addRelationshipType"
        @close="closeAddMemberDrawer"
        @saved="handleMemberSaved"
      />
    </v-navigation-drawer>

    <!-- Member Edit Drawer -->
    <v-navigation-drawer v-model="editMemberDrawer" location="right" temporary width="650">
      <MemberEditView
        v-if="selectedMemberIdForEdit && editMemberDrawer"
        :member-id="selectedMemberIdForEdit"
        @close="closeEditMemberDrawer"
        @saved="handleMemberSaved"
      />
    </v-navigation-drawer>

    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog
      :model-value="deleteConfirmDialog"
      :title="t('confirmDelete.title')"
      :message="t('member.list.confirmDelete', { fullName: memberToDelete?.fullName || '' })"
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { TreeChart } from '@/components/family';
import MemberAddView from '@/views/member/MemberAddView.vue';
import MemberEditView from '@/views/member/MemberEditView.vue';
import { ConfirmDeleteDialog } from '@/components/common';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import type { Member } from '@/types';
import { RelationshipType } from '@/types';

const { t } = useI18n();
const memberStore = useMemberStore();
const notificationStore = useNotificationStore();

const selectedFamilyId = ref<string | null>(null);

// State for Add Member Drawer
const addMemberDrawer = ref(false);
const parentMemberId = ref<string | null>(null);
const addRelationshipType = ref<RelationshipType | null>(null);

// State for Edit Member Drawer
const editMemberDrawer = ref(false);
const selectedMemberIdForEdit = ref<string | null>(null);

// State for Delete Member Dialog
const deleteConfirmDialog = ref(false);
const memberToDelete = ref<Member | undefined>(undefined);

const onFamilyChange = (familyId: string | null) => {
  selectedFamilyId.value = familyId;
  // Optionally, refresh the tree data when family changes
  // memberStore.fetchTreeData(familyId); // Assuming such a method exists
};

// --- Add Member Handlers ---
const openAddMemberDrawer = () => {
  console.log('fire');
  
  parentMemberId.value = null;
  addRelationshipType.value = null;
  addMemberDrawer.value = true;
};

const openAddFatherDrawer = (memberId: string) => {
  parentMemberId.value = memberId;
  addRelationshipType.value = RelationshipType.Father;
  addMemberDrawer.value = true;
};

const openAddMotherDrawer = (memberId: string) => {
  parentMemberId.value = memberId;
  addRelationshipType.value = RelationshipType.Mother;
  addMemberDrawer.value = true;
};

const openAddChildDrawer = (memberId: string) => {
  parentMemberId.value = memberId;
  addRelationshipType.value = RelationshipType.Child; // Assuming a Child relationship type
  addMemberDrawer.value = true;
};

const closeAddMemberDrawer = () => {
  addMemberDrawer.value = false;
  parentMemberId.value = null;
  addRelationshipType.value = null;
};

// --- Edit Member Handlers ---
const openEditMemberDrawer = (memberId: string) => {
  selectedMemberIdForEdit.value = memberId;
  editMemberDrawer.value = true;
};

const closeEditMemberDrawer = () => {
  editMemberDrawer.value = false;
  selectedMemberIdForEdit.value = null;
};

// --- Delete Member Handlers ---
const confirmDeleteMember = async (memberId: string) => {
  // Fetch member details to display name in confirmation dialog
  const member = await memberStore.getById(memberId);
  if (member) {
    memberToDelete.value = member;
    deleteConfirmDialog.value = true;
  } else {
    notificationStore.showSnackbar(t('member.errors.loadById'), 'error');
  }
};

const handleDeleteConfirm = async () => {
  if (memberToDelete.value && memberToDelete.value.id) {
    await memberStore.deleteItem(memberToDelete.value.id);
    if (memberStore.error) {
      notificationStore.showSnackbar(
        memberStore.error || t('member.messages.deleteError'),
        'error',
      );
    } else {
      notificationStore.showSnackbar(
        t('member.messages.deleteSuccess'),
        'success',
      );
      // Refresh tree data after deletion
      // memberStore.fetchTreeData(selectedFamilyId.value); // Assuming such a method exists
    }
  }
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

// --- General Handlers ---
const handleMemberSaved = () => {
  // Close all drawers/dialogs and refresh the tree
  closeAddMemberDrawer();
  closeEditMemberDrawer();
  // Refresh tree data after saving
  // memberStore.fetchTreeData(selectedFamilyId.value); // Assuming such a method exists
};
</script>
<style>
.tree-filter-input {
  margin-top: 3px;
  max-width: 320px;
}
</style>