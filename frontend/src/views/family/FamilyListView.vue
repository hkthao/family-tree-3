<template>
  <v-container fluid>
    <FamilySearch @update:filters="handleFilterUpdate" />

    <FamilyList
      :families="families"
      :total-families="totalFamilies"
      :loading="loading"
      :items-per-page="itemsPerPage"
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
import { useFamilies } from '@/data/families';
import type { Family, FamilyFilter } from '@/types/family';
import FamilySearch from '@/components/family/FamilySearch.vue';
import FamilyList from '@/components/family/FamilyList.vue';
import FamilyForm from '@/components/family/FamilyForm.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import { useNotificationStore } from '@/stores/notification';

const { t } = useI18n();
const router = useRouter();
const { getFamilies, deleteFamily } = useFamilies();
const notificationStore = useNotificationStore();

const families = ref<Family[]>([]);
const totalFamilies = ref(0);
const loading = ref(true);
const currentFilters = ref<FamilyFilter>({});
const currentPage = ref(1);
const itemsPerPage = ref(10);

const detailDialog = ref(false);
const selectedFamily = ref<Family | undefined>(undefined);
const deleteConfirmDialog = ref(false);
const familyToDelete = ref<Family | undefined>(undefined);

const loadFamilies = async () => {
  loading.value = true;
  const { families: fetchedFamilies, total } = await getFamilies(
    currentFilters.value.fullName,
    currentFilters.value.visibility,
    currentPage.value,
    itemsPerPage.value
  );
  families.value = fetchedFamilies;
  totalFamilies.value = total;
  loading.value = false;
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
    loading.value = true;
    try {
      await deleteFamily(familyToDelete.value.id);
      notificationStore.showSnackbar(t('family.management.messages.deleteSuccess'), 'success');
      await loadFamilies();
    } catch (error) {
      notificationStore.showSnackbar(t('family.management.messages.deleteError'), 'error');
    }
    loading.value = false;
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
});
</script>