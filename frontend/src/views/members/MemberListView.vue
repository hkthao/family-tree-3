<template>
  <v-container fluid>
    <MemberSearch @update:filters="handleFilterUpdate" :families="familiesStore.items" />

    <MemberList
      :members="membersStore.items"
      :total-members="membersStore.total"
      :loading="membersStore.loading"
      :families="familiesStore.items"
      @update:options="handleListOptionsUpdate"
      @view="openViewDialog"
      @edit="navigateToEditMember"
      @delete="confirmDelete"
      @create="navigateToCreateView"
    />

    <v-dialog v-model="viewDialog" max-width="800px">
            <MemberForm
              v-if="selectedMemberForView"
              :initial-member-data="selectedMemberForView"
              :read-only="true"
              :title="t('member.form.title')"
              :members="membersStore.items"
              :families="familiesStore.items"
              @close="closeViewDialog"
            />    </v-dialog>

    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog
      :model-value="deleteConfirmDialog"
      :title="t('confirmDelete.title')"
      :message="t('member.list.confirmDelete', { fullName: memberToDelete?.fullName || '' })"
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />

    <!-- Global Snackbar -->
    <v-snackbar v-if="notificationStore.snackbar" v-model="notificationStore.snackbar.show" :color="notificationStore.snackbar.color" timeout="3000">
      {{ notificationStore.snackbar.message }}
    </v-snackbar>

  
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMembersStore } from '@/stores/members';
import { useFamiliesStore } from '@/stores/families';
import type { Member, MemberFilter } from '@/types/member';
import type { Family } from '@/types/family';
import MemberSearch from '@/components/members/MemberSearch.vue';
import MemberList from '@/components/members/MemberList.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import MemberForm from '@/components/members/MemberForm.vue';

const { t } = useI18n();
const membersStore = useMembersStore();
const familiesStore = useFamiliesStore();

const allMembers = ref<Member[]>([]);
const currentFilters = ref<MemberFilter>({});
const currentPage = ref(1);
const itemsPerPage = ref(10);

import { useRouter } from 'vue-router';

const router = useRouter();

const deleteConfirmDialog = ref(false); // Re-add deleteConfirmDialog
const memberToDelete = ref<Member | undefined>(undefined); // Add memberToDelete ref
const viewDialog = ref(false);
const selectedMemberForView = ref<Member | null>(null);

import { useNotificationStore } from '@/stores/notification';

const notificationStore = useNotificationStore();

// Function Declarations (moved to top)
const loadMembers = async () => {
  await membersStore.fetchAll(
    currentFilters.value.fullName,
    currentPage.value,
    itemsPerPage.value
  );
};

const loadAllMembers = async () => {
  await membersStore.fetchAll({}, 1, -1); // Fetch all members
  allMembers.value = membersStore.items;
};

const loadFamilies = async () => {
  await familiesStore.fetchAll('', 1, -1);
};

const openViewDialog = (member: Member) => {
  selectedMemberForView.value = member;
  viewDialog.value = true;
};

const closeViewDialog = () => {
  viewDialog.value = false;
  selectedMemberForView.value = null;
};

const navigateToCreateView = () => {
  router.push('/members/add');
};

const navigateToEditMember = (member: Member) => {
  router.push(`/members/edit/${member.id}`);
};

const handleFilterUpdate = (filters: MemberFilter) => {
  currentFilters.value = filters;
  currentPage.value = 1; // Reset to first page on filter change
  loadMembers();
};

const handleListOptionsUpdate = (options: { page: number; itemsPerPage: number }) => {
  currentPage.value = options.page;
  itemsPerPage.value = options.itemsPerPage;
  loadMembers();
};

const confirmDelete = (member: Member) => {
  memberToDelete.value = member;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (memberToDelete.value) {
    try {
      await membersStore.remove(memberToDelete.value.id);
      notificationStore.showSnackbar(t('member.messages.deleteSuccess'), 'success');
      await membersStore.fetchAll(); // Reload members after deletion
    } catch (error) {
      notificationStore.showSnackbar(t('member.messages.deleteError'), 'error');
    }
  }
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

onMounted(() => {
  loadMembers();
  loadAllMembers();
  loadFamilies();
});
</script>
