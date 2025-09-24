<template>
  <v-container fluid>
    <FamilySearch @update:filters="handleFilterUpdate" />

    <FamilyList
      :items="items"
      :total-items="familyStore.totalItems"
      :loading="familyStore.loading"
      :items-per-page="itemsPerPage"
      :family-member-counts="familyMemberCounts"
      @update:options="handleListOptionsUpdate"
      @update:itemsPerPage="itemsPerPage = $event"
      @view="navigateToViewFamily"
      @edit="navigateToEditFamily"
      @delete="confirmDelete"
      @create="navigateToAddFamily"
    />

    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog
      :model-value="deleteConfirmDialog"
      :title="t('confirmDelete.title')"
      :message="t('confirmDelete.message', { name: familyToDelete?.name || '' })"
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />

    <!-- Family Detail Dialog -->
    <v-dialog v-model="detailDialog" max-width="600px" persistent>
      <FamilyForm
        :initial-family-data="selectedFamily"
        :title="t('family.detail.title')"
        :read-only="true"
        @cancel="closeDetail"
      />
    </v-dialog>

    <!-- Snackbar -->
    <v-snackbar v-model="notificationStore.snackbar.show" :color="notificationStore.snackbar.color" timeout="3000">
      {{ notificationStore.snackbar.message }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
const familyStore = useFamilyStore();
import { useMemberStore } from '@/stores/member.store';
import type { Family } from '@/types/family';
import type { FamilyFilter } from '@/services/family/family.service.interface';

import FamilySearch from '@/components/family/FamilySearch.vue';
import FamilyList from '@/components/family/FamilyList.vue';
import FamilyForm from '@/components/family/FamilyForm.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import { useNotificationStore } from '@/stores/notification.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

const { t } = useI18n();
const router = useRouter();

const { items } = storeToRefs(familyStore);
const membersStore = useMemberStore();
const notificationStore = useNotificationStore();

const currentFilters = ref<FamilyFilter>({});
const currentPage = ref(1);
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const detailDialog = ref(false);
const selectedFamily = ref<Family | undefined>(undefined);
const deleteConfirmDialog = ref(false);
const familyToDelete = ref<Family | undefined>(undefined);

const familyMemberCounts = computed(() => {
  const counts: { [key: string]: number } = {};
  membersStore.items.forEach(member => {
    if (member.familyId) {
      counts[member.familyId] = (counts[member.familyId] || 0) + 1;
    }
  });
  return counts;
});

const loadFamilies = async () => {
  await familyEventStore.searchItems(
    currentFilters.value.name || '',
    currentFilters.value.familyId || undefined,
  );
};

const loadAllMembers = async () => {
  await membersStore.fetchItems(); // Fetch all members
};

const handleFilterUpdate = (filters: FamilyFilter) => {
  currentFilters.value = filters;
  currentPage.value = 1; // Reset to first page on filter change
  loadFamilies();
};

const handleListOptionsUpdate = (options: { page: number; itemsPerPage: number }) => {
  familyStore.setPage(options.page);
  familyStore.setItemsPerPage(options.itemsPerPage);
};

const navigateToAddFamily = () => {
  router.push('/family/add');
};

const navigateToEditFamily = (family: Family) => {
  router.push(`/family/edit/${family.id}`);
};

const navigateToViewFamily = (family: Family) => {
  selectedFamily.value = { ...family };
  detailDialog.value = true;
};

const closeDetail = () => {
  detailDialog.value = false;
  selectedFamily.value = undefined;
};

const confirmDelete = (family: Family) => {
  familyToDelete.value = family;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (familyToDelete.value) {
    try {
      await familyStore.deleteItem(familyToDelete.value.id);
      notificationStore.showSnackbar(t('family.management.messages.deleteSuccess'), 'success');
      await familyStore._loadItems(); // Reload families after deletion
    } catch (error) {
      notificationStore.showSnackbar(t('family.management.messages.deleteError'), 'error');
    }
  }
  deleteConfirmDialog.value = false;
  familyToDelete.value = undefined;
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  familyToDelete.value = undefined;
};

watch([currentFilters, currentPage, itemsPerPage], () => {
  loadFamilies();
});

onMounted(async () => {
  await loadFamilies();
  await loadAllMembers();
});


</script>