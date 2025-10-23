<template>
  <MemberSearch @update:filters="handleFilterUpdate" />

  <MemberList :items="memberStore.items" :total-items="memberStore.totalItems" :loading="loading"
    @update:options="handleListOptionsUpdate" @view="navigateToDetailView" @edit="navigateToEditMember"
    @delete="confirmDelete" @create="navigateToCreateView" @ai-biography="navigateToAIBiography" @ai-create="openAiInputDialog" />

  <!-- Confirm Delete Dialog -->
  <ConfirmDeleteDialog :model-value="deleteConfirmDialog" :title="t('confirmDelete.title')" :message="t('member.list.confirmDelete', {
    fullName: memberToDelete?.fullName || '',
  })
    " @confirm="handleDeleteConfirm" @cancel="handleDeleteCancel" />

  <!-- AI Input Dialog -->
  <NLMemberPopup :model-value="aiInputDialog" @update:model-value="aiInputDialog = $event" @saved="handleAiSaved" />

  <!-- Global Snackbar -->
  <v-snackbar v-if="notificationStore.snackbar" v-model="notificationStore.snackbar.show"
    :color="notificationStore.snackbar.color" timeout="3000">
    {{ notificationStore.snackbar.message }}
  </v-snackbar>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import { MemberSearch, MemberList, NLMemberPopup } from '@/components/members';
import { ConfirmDeleteDialog } from '@/components/common';
import { useNotificationStore } from '@/stores/notification.store';
import { useRouter } from 'vue-router';
import type { MemberFilter, Member } from '@/types';

const { t } = useI18n();
const router = useRouter();
const memberStore = useMemberStore();
const { loading } = storeToRefs(memberStore);
const deleteConfirmDialog = ref(false); // Re-add deleteConfirmDialog
const memberToDelete = ref<Member | undefined>(undefined); // Add memberToDelete ref
const aiInputDialog = ref(false);

const notificationStore = useNotificationStore();

const navigateToDetailView = (member: Member) => {
  router.push(`/members/detail/${member.id}`);
};

const navigateToCreateView = () => {
  router.push('/members/add');
};

const navigateToEditMember = (member: Member) => {
  router.push(`/members/edit/${member.id}`);
};

const navigateToAIBiography = (member: Member) => {
  router.push(`/members/biography/${member.id}`);
};

const handleFilterUpdate = async (filters: MemberFilter) => {
  memberStore.filters = filters;
  await memberStore._loadItems()
};

const handleListOptionsUpdate = async (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];  
}) => {
  await memberStore.setPage(options.page);
  await memberStore.setItemsPerPage(options.itemsPerPage);
  // Handle sorting
  if (options.sortBy && options.sortBy.length > 0) {
    memberStore.setSortBy(options.sortBy);
  } else {
    memberStore.setSortBy([]); // Clear sort if no sortBy is provided
  }
};

const confirmDelete = (member: Member) => {
  memberToDelete.value = member;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (memberToDelete.value) {
    await memberStore.deleteItem(memberToDelete.value.id);
    if (memberStore.error) {
      notificationStore.showSnackbar(
        t('member.messages.deleteError', { error: memberStore.error }),
        'error',
      );
    } else {
      notificationStore.showSnackbar(
        t('member.messages.deleteSuccess'),
        'success',
      );
    }
  }
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  memberToDelete.value = undefined;
};

const openAiInputDialog = () => {
  aiInputDialog.value = true;
};

const handleAiSaved = () => {
  memberStore._loadItems(); // Refresh the member list after saving
};

onMounted(() => {
  memberStore._loadItems();
})

</script>
