<template>
  <v-container fluid>
    <MemberSearch @update:filters="handleFilterUpdate" />

    <MemberList
      :members="members"
      :total-members="totalMembers"
      :loading="loading"
      @update:options="handleListOptionsUpdate"
      @view="navigateToDetailView"
      @edit="navigateToEditMember"
      @delete="confirmDelete"
    />



    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog
      :model-value="deleteConfirmDialog"
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
import { useMembers } from '@/data/members';
import type { Member, MemberFilter } from '@/types/member';
import MemberSearch from '@/components/members/MemberSearch.vue';
import MemberList from '@/components/members/MemberList.vue';
import ConfirmDeleteDialog from '@/components/family/ConfirmDeleteDialog.vue';

const { t } = useI18n();
const { getMembers, deleteMember } = useMembers();

const members = ref<Member[]>([]);
const totalMembers = ref(0);
const loading = ref(true);
const currentFilters = ref<MemberFilter>({});
const currentPage = ref(1);
const itemsPerPage = ref(10);

import { useRouter } from 'vue-router';

const router = useRouter();

const deleteConfirmDialog = ref(false); // Re-add deleteConfirmDialog
const memberToDelete = ref<Member | undefined>(undefined); // Add memberToDelete ref

import { useNotificationStore } from '@/stores/notification';

const notificationStore = useNotificationStore();

// Function Declarations (moved to top)
const loadMembers = async () => {
  loading.value = true;
  const { members: fetchedMembers, total } = await getMembers(
    currentFilters.value,
    currentPage.value,
    itemsPerPage.value
  );
  members.value = fetchedMembers;
  totalMembers.value = total;
  loading.value = false;
};

const navigateToDetailView = (member: Member) => {
  router.push(`/members/${member.id}`);
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
    loading.value = true;
    try {
      await deleteMember(memberToDelete.value.id);
      notificationStore.showSnackbar(t('member.messages.deleteSuccess'), 'success');
      await loadMembers();
    } catch (error) {
      notificationStore.showSnackbar(t('member.messages.deleteError'), 'error');
    }
    loading.value = false;
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
});
</script>
