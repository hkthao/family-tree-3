<template>
  <v-container >
    <v-card >
      <v-card-title class="d-flex align-center">
        {{ $t('family.management.title') }}
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="openAddForm">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </v-card-title>

      <v-card-text>
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field
              v-model="searchQuery"
              :label="$t('family.management.searchLabel')"
              clearable
              prepend-inner-icon="mdi-magnify"
              variant="outlined"
              density="compact"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-select
              v-model="filterVisibility"
              :items="visibilityItems"
              :label="$t('family.management.filterLabel')"
              variant="outlined"
              density="compact"
            ></v-select>
          </v-col>
        </v-row>

        <v-data-table-server
          v-model:items-per-page="itemsPerPage"
          :headers="headers"
          :items="families"
          :items-length="totalFamilies"
          :loading="loading"
          item-value="id"
          @update:options="loadFamilies"
          elevation="0"
        >
          <template #item.AvatarUrl="{ item }">
            <div class="d-flex justify-center">
              <v-avatar size="36" class="my-2">
                <v-img v-if="item.AvatarUrl" :src="item.AvatarUrl" :alt="item.Name"></v-img>
                <v-icon v-else>mdi-account-group</v-icon>
              </v-avatar>
            </div>
          </template>
          <template #item.Name="{ item }">
            <div class="text-left">
              <v-btn variant="text" color="primary" @click.prevent="viewFamily(item)" class="text-none">{{ item.Name }}</v-btn>
            </div>
          </template>
          <template #item.Visibility="{ item }">
            <v-chip :color="item.Visibility === 'Public' ? 'success' : 'error'" label>{{ $t(`family.management.visibility.${item.Visibility.toLowerCase()}`) }}</v-chip>
          </template>
          <template #item.actions="{ item }">
            <v-btn icon size="small" variant="text" @click="editFamily(item)">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
            <v-btn icon size="small" variant="text" @click="confirmDelete(item)">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </template>
          <template #loading>
            <v-skeleton-loader type="table-row@5"></v-skeleton-loader>
          </template>
        </v-data-table-server>
      </v-card-text>
    </v-card>

    <!-- Family Form Dialog -->
    <v-dialog v-model="formDialog" max-width="600px" persistent>
      <FamilyForm
        :family="selectedFamily"
        @save="handleSaveFamily"
        @cancel="closeForm"
      />
    </v-dialog>

    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog
      :model-value="deleteConfirmDialog"
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />

    <!-- Family Detail Dialog -->
    <v-dialog v-model="detailDialog" max-width="600px" persistent>
      <FamilyDetail
        :family="selectedFamily"
        @back="closeDetail"
      />
    </v-dialog>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000">
      {{ snackbar.message }}
    </v-snackbar>
    </v-container>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilies } from '@/data/families';
import type { Family } from '@/data/families';
import FamilyForm from './FamilyForm.vue';
import ConfirmDeleteDialog from './ConfirmDeleteDialog.vue';
import FamilyDetail from './FamilyDetail.vue';

const { t } = useI18n();
const { getFamilies, addFamily, updateFamily, deleteFamily } = useFamilies();

const families = ref<Family[]>([]);
const totalFamilies = ref(0);
const loading = ref(true);
const searchQuery = ref('');
const filterVisibility = ref<'All' | 'Private' | 'Public'>('All');
const itemsPerPage = ref(5);

const formDialog = ref(false);
const deleteConfirmDialog = ref(false);
const detailDialog = ref(false);
const selectedFamily = ref<Family | undefined>(undefined);
const familyToDelete = ref<Family | undefined>(undefined);

const snackbar = ref({
  show: false,
  message: '',
  color: '',
});

const headers = computed(() => [
  { title: t('family.management.headers.avatar'), key: 'AvatarUrl', sortable: false, width: '120px', align: 'center' },
  { title: t('family.management.headers.name'), key: 'Name', width: 'auto', align: 'center' },
  { title: t('family.management.headers.visibility'), key: 'Visibility', width: '120px', align: 'center' },
  { title: t('family.management.headers.actions'), key: 'actions', sortable: false, width: '120px', align: 'center' },
]);

const visibilityItems = computed(() => [
  { title: t('family.management.visibility.all'), value: 'All' },
  { title: t('family.management.visibility.private'), value: 'Private' },
  { title: t('family.management.visibility.public'), value: 'Public' },
]);

const loadFamilies = async ({ page, itemsPerPage: perPage }: { page: number; itemsPerPage: number }) => {
  loading.value = true;
  const { families: fetchedFamilies, total } = await getFamilies(
    searchQuery.value,
    filterVisibility.value,
    page,
    perPage
  );
  families.value = fetchedFamilies;
  totalFamilies.value = total;
  loading.value = false;
};

const openAddForm = () => {
  selectedFamily.value = undefined;
  formDialog.value = true;
};

const editFamily = (family: Family) => {
  selectedFamily.value = { ...family };
  formDialog.value = true;
};

const viewFamily = (family: Family) => {
  selectedFamily.value = { ...family };
  detailDialog.value = true;
};

const closeForm = () => {
  formDialog.value = false;
  selectedFamily.value = undefined;
};

const closeDetail = () => {
  detailDialog.value = false;
  selectedFamily.value = undefined;
};

const handleSaveFamily = async (familyData: Omit<Family, 'id'> & { id?: number }) => {
  loading.value = true;
  try {
    if (familyData.id) {
      await updateFamily(familyData as Family);
      snackbar.value = { show: true, message: t('family.management.messages.updateSuccess'), color: 'success' };
    } else {
      await addFamily(familyData);
      snackbar.value = { show: true, message: t('family.management.messages.addSuccess'), color: 'success' };
    }
    closeForm();
    await loadFamilies({ page: 1, itemsPerPage: itemsPerPage.value });
  } catch (error) {
    snackbar.value = { show: true, message: t('family.management.messages.saveError'), color: 'error' };
  }
  loading.value = false;
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
      snackbar.value = { show: true, message: t('family.management.messages.deleteSuccess'), color: 'success' };
      await loadFamilies({ page: 1, itemsPerPage: itemsPerPage.value });
    } catch (error) {
      snackbar.value = { show: true, message: t('family.management.messages.deleteError'), color: 'error' };
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

watch([searchQuery, filterVisibility], () => {
  loadFamilies({ page: 1, itemsPerPage: itemsPerPage.value });
});

onMounted(() => {
  loadFamilies({ page: 1, itemsPerPage: itemsPerPage.value });
});
</script>