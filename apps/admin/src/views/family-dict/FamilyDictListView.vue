<template>
  <div data-testid="family-dict-list-view">
    <FamilyDictSearch v-if="!props.hideSearch" @update:filters="handleFilterUpdate" />

    <FamilyDictList :items="familyDictStore.list.items" :total-items="familyDictStore.list.totalItems"
      :loading="list.loading" :search="searchQuery" @update:search="handleSearchUpdate"
      @update:options="handleListOptionsUpdate" @view="navigateToDetailView" @edit="navigateToEditFamilyDict"
      @delete="confirmDelete" @create="navigateToCreateView()" :read-only="props.readOnly">
    </FamilyDictList>

    <!-- Edit FamilyDict Drawer -->
    <v-navigation-drawer v-model="editDrawer" location="right" temporary width="650">
      <FamilyDictEditView v-if="selectedFamilyDictId && editDrawer" :family-dict-id="selectedFamilyDictId"
        @close="handleFamilyDictClosed" @saved="handleFamilyDictSaved" />
    </v-navigation-drawer>

    <!-- Add FamilyDict Drawer -->
    <v-navigation-drawer v-model="addDrawer" location="right" temporary width="650">
      <FamilyDictAddView v-if="addDrawer" @close="handleFamilyDictClosed" @saved="handleFamilyDictSaved" />
    </v-navigation-drawer>

    <!-- Detail FamilyDict Drawer -->
    <v-navigation-drawer v-model="detailDrawer" location="right" temporary width="650">
      <FamilyDictDetailView v-if="selectedFamilyDictId && detailDrawer" :family-dict-id="selectedFamilyDictId"
        @close="handleDetailClosed" @edit-family-dict="navigateToEditFamilyDict" />
    </v-navigation-drawer>
  </div>
</template>

<script setup lang="ts">
import { useFamilyDictStore } from '@/stores/family-dict.store';
import { FamilyDictSearch, FamilyDictList } from '@/components/family-dict';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import FamilyDictEditView from '@/views/family-dict/FamilyDictEditView.vue';
import FamilyDictAddView from '@/views/family-dict/FamilyDictAddView.vue';
import FamilyDictDetailView from '@/views/family-dict/FamilyDictDetailView.vue';
import type { FamilyDictFilter, FamilyDict } from '@/types';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { onMounted, ref } from 'vue';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // Import useGlobalSnackbar

interface FamilyDictListViewProps {
  readOnly?: boolean;
  hideSearch?: boolean;
}

const props = defineProps<FamilyDictListViewProps>();

const { t } = useI18n();
const familyDictStore = useFamilyDictStore();
const { list } = storeToRefs(familyDictStore);
const searchQuery = ref('');
const editDrawer = ref(false);
const addDrawer = ref(false);
const selectedFamilyDictId = ref<string | null>(null);
const detailDrawer = ref(false);

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar(); // Khởi tạo useGlobalSnackbar

const navigateToDetailView = (familyDict: FamilyDict) => {
  selectedFamilyDictId.value = familyDict.id;
  detailDrawer.value = true;
};

const navigateToCreateView = () => {
  addDrawer.value = true;
};

const navigateToEditFamilyDict = (familyDict: FamilyDict) => {
  selectedFamilyDictId.value = familyDict.id;
  detailDrawer.value = false;
  editDrawer.value = true;
};

const handleFilterUpdate = async (filters: FamilyDictFilter) => {
  familyDictStore.list.filters = { ...filters, searchQuery: searchQuery.value };
  await familyDictStore._loadItems()
};

const handleSearchUpdate = async (search: string) => {
  searchQuery.value = search;
  familyDictStore.list.filters = { ...familyDictStore.list.filters, searchQuery: searchQuery.value };
  await familyDictStore._loadItems();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  familyDictStore.setListOptions(options);
};

const confirmDelete = async (familyDict: FamilyDict) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyDict.list.confirmDelete', { name: familyDict.name || '' }),
    confirmText: t('common.delete'),
    cancelText: 'common.cancel',
    confirmColor: 'error',
  });

  if (confirmed) {
    await handleDeleteConfirm(familyDict);
  }
};

const handleDeleteConfirm = async (familyDict: FamilyDict) => {
  if (familyDict) {
    await familyDictStore.deleteItem(familyDict.id);
    if (familyDictStore.error) {
      showSnackbar(
        t('familyDict.messages.deleteError', { error: familyDictStore.error }),
        'error',
      );
    } else {
      showSnackbar(
        t('familyDict.messages.deleteSuccess'),
        'success',
      );
    }
  }
  familyDictStore._loadItems();
};

const handleFamilyDictSaved = () => {
  editDrawer.value = false;
  addDrawer.value = false;
  selectedFamilyDictId.value = null;
  familyDictStore._loadItems();
};

const handleFamilyDictClosed = () => {
  editDrawer.value = false;
  addDrawer.value = false;
  selectedFamilyDictId.value = null;
};

const handleDetailClosed = () => {
  detailDrawer.value = false;
  selectedFamilyDictId.value = null;
};

onMounted(() => {
  familyDictStore._loadItems();
})

</script>
