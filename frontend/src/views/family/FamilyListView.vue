<template>
  <v-container fluid>
    <FamilySearch @update:filters="handleFilterUpdate" />

    <FamilyList
      :families="familiesStore.items"
      :total-families="familiesStore.total"
      :loading="familiesStore.loading"
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
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamiliesStore } from '@/stores/families';
import { useMembersStore } from '@/stores/members';
import type { Family, FamilyFilter } from '@/types/family';
import type { Member } from '@/types/member';
import FamilySearch from '@/components/family/FamilySearch.vue';
import FamilyList from '@/components/family/FamilyList.vue';
import FamilyForm from '@/components/family/FamilyForm.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import { useNotificationStore } from '@/stores/notification';

const { t } = useI18n();
const router = useRouter();
const familiesStore = useFamiliesStore();
const membersStore = useMembersStore();
const notificationStore = useNotificationStore();

const allMembers = ref<Member[]>([]);
const currentFilters = ref<FamilyFilter>({});
const currentPage = ref(1);
const itemsPerPage = ref(10);

const detailDialog = ref(false);
const selectedFamily = ref<Family | undefined>(undefined);
const deleteConfirmDialog = ref(false);
const familyToDelete = ref<Family | undefined>(undefined);

const familyMemberCounts = computed(() => {
  const counts: { [key: string]: number } = {};
  allMembers.value.forEach(member => {
    if (member.familyId) {
      counts[member.familyId] = (counts[member.familyId] || 0) + 1;
    }
  });
  return counts;
});

const loadFamilies = async () => {
  await familiesStore.fetchAll(
    currentFilters.value.fullName,
    currentPage.value,
    itemsPerPage.value
  );
};

const loadAllMembers = async () => {
  await membersStore.fetchAll({}, 1, -1); // Fetch all members
  allMembers.value = membersStore.items;
};

const handleFilterUpdate = (filters: FamilyFilter) => {
  currentFilters.value = filters;
  currentPage.value = 1; // Reset to first page on filter change
  loadFamilies();
};

const handleListOptionsUpdate = (options: { page: number; itemsPerPage: number }) => {
  currentPage.value = options.page;
  itemsPerPage.value = options.itemsPerPage;
  loadFamilies();
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
      await familiesStore.remove(familyToDelete.value.id);
      notificationStore.showSnackbar(t('family.management.messages.deleteSuccess'), 'success');
      await familiesStore.fetchAll(); // Reload families after deletion
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

onMounted(() => {
  loadFamilies();
  loadAllMembers();
});
</script>