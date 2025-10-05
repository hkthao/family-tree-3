<template>
  <v-card>
    <v-card-title>{{ t('relationship.list.title') }}</v-card-title>
    <v-card-text>
      <v-data-table :headers="headers" :items="relationshipStore.items" :loading="relationshipStore.loading"
        :items-per-page="relationshipStore.itemsPerPage" :total-items="relationshipStore.totalItems"
        @update:options="loadItems">
        <template v-slot:item.sourceMemberFullName="{ item }">
          <a @click="navigateToMemberDetail(item.sourceMemberId)"
            class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
            {{ item.sourceMemberFullName }}
          </a>
        </template>
        <template v-slot:item.targetMemberFullName="{ item }">
          <a @click="navigateToMemberDetail(item.targetMemberId)"
            class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
            {{ item.targetMemberFullName }}
          </a>
        </template>
        <template v-slot:item.type="{ item }">
          {{ getRelationshipTypeTitle(item.type) }}
        </template>
        <template v-slot:item.actions="{ item }">
          <v-icon small class="mr-2" @click="editItem(item)">mdi-pencil</v-icon>
          <v-icon small @click="deleteItem(item)">mdi-delete</v-icon>
        </template>
      </v-data-table>
    </v-card-text>
  </v-card>
  <ConfirmDeleteDialog v-model="deleteConfirmDialog" :title="t('confirmDelete.title')"
    :message="t('confirmDelete.message', { name: relationshipToDelete?.id || '' })" @confirm="handleDeleteConfirm"
    @cancel="handleDeleteCancel" />
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useRelationshipStore } from '@/stores/relationship.store';
import { useNotificationStore } from '@/stores/notification.store';
import type { Relationship } from '@/types'; // Added Relationship
import { ConfirmDeleteDialog } from '@/components/common';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes'; // Added

const { t } = useI18n();
const relationshipStore = useRelationshipStore();
const router = useRouter();
const notificationStore = useNotificationStore();

const headers = computed(() => [
  { title: t('relationship.list.headers.sourceMember'), key: 'sourceMemberFullName', sortable: true },
  { title: t('relationship.list.headers.targetMember'), key: 'targetMemberFullName', sortable: true },
  { title: t('relationship.list.headers.type'), key: 'type', sortable: true },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);

const navigateToMemberDetail = (memberId: string) => {
  router.push({ name: 'MemberDetail', params: { id: memberId } });
};

const loadItems = async ({ page, itemsPerPage, sortBy }: { page: number; itemsPerPage: number; sortBy: any[] }) => {
  await relationshipStore.setPage(page);
  await relationshipStore.setItemsPerPage(itemsPerPage);
  // You might need to implement sorting in your store
  await relationshipStore._loadItems();
};

const editItem = (item: Relationship) => {
  router.push({ name: 'EditRelationship', params: { id: item.id } });
};

const deleteConfirmDialog = ref(false);
const relationshipToDelete = ref<Relationship | null>(null);

const confirmDelete = (item: Relationship) => {
  relationshipToDelete.value = item;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (relationshipToDelete.value) {
    const result = await relationshipStore.deleteItem(relationshipToDelete.value.id);
    if (result.ok) {
      notificationStore.showSnackbar(t('relationship.messages.deleteSuccess'), 'success');
    } else {
      notificationStore.showSnackbar(result.error?.message || t('relationship.messages.deleteError'), 'error');
    }
    deleteConfirmDialog.value = false;
    relationshipToDelete.value = null;
  }
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  relationshipToDelete.value = null;
};

const deleteItem = (item: Relationship) => {
  confirmDelete(item);
};
</script>
