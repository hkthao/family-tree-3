<template>
  <div>
    <MemberStoryList
      :items="memberStoryStore.list.items"
      :total-items="memberStoryStore.list.totalItems"
      :loading="memberStoryStore.list.loading"
      :items-per-page="memberStoryStore.list.itemsPerPage"
      :search="searchQuery"
      @update:options="handleListOptionsUpdate"
      @update:search="handleSearchUpdate"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      @create="openAddDrawer"
    />

    <!-- Add MemberStory Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMemberStoryClosed">
      <MemberStoryAddView v-if="addDrawer" @close="handleMemberStoryClosed" @saved="handleMemberStorySaved" />
    </BaseCrudDrawer>

    <!-- Edit MemberStory Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleMemberStoryClosed">
      <MemberStoryEditView v-if="selectedItemId && editDrawer" :member-story-id="selectedItemId" @close="handleMemberStoryClosed" @saved="handleMemberStorySaved" />
    </BaseCrudDrawer>

    <!-- Detail MemberStory Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleMemberStoryClosed">
      <MemberStoryDetailView v-if="selectedItemId && detailDrawer" :member-story-id="selectedItemId"
        @close="handleMemberStoryClosed" @edit-item="openEditDrawer" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { watch, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useCrudDrawer } from '@/composables/useCrudDrawer';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useMemberStoryStore } from '@/stores/memberStory.store';
import type { MemberStoryDto } from '@/types/memberStory';
import MemberStoryAddView from './MemberStoryAddView.vue';
import MemberStoryEditView from './MemberStoryEditView.vue'; // NEW IMPORT
import MemberStoryDetailView from './MemberStoryDetailView.vue';
import MemberStoryList from '@/components/member-story/MemberStoryList.vue';
import { removeDiacritics } from '@/utils/string.utils'; // NEW IMPORT
import { useAuth } from '@/composables/useAuth'; // NEW IMPORT

interface MemberStoryListViewProps {
  memberId?: string;
  familyId?: string;
  readOnly?: boolean;
}

const props = defineProps<MemberStoryListViewProps>();

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore();
const searchQuery = ref(''); // Use a ref to hold the current search query for filtering

// No need to destructure isAdmin, isFamilyManager if not used directly here
useAuth(); // NEW: Use auth composable (just call it if no destructuring needed)

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer, // Use directly from useCrudDrawer
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const confirmDelete = async (item: MemberStoryDto) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('memberStory.list.confirmDelete', { title: item.title || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    await handleDeleteConfirm(item);
  }
};

const handleDeleteConfirm = async (item: MemberStoryDto) => {
  if (item && item.id) {
    await memberStoryStore.deleteItem(item.id);
    if (memberStoryStore.error) {
      showSnackbar(
        t('memberStory.messages.deleteError', { error: memberStoryStore.error }),
        'error',
      );
    } else {
      showSnackbar(
        t('memberStory.messages.deleteSuccess'),
        'success',
      );
    }
  } else {
    showSnackbar(
      t('memberStory.messages.deleteError', { error: t('common.invalidId') }),
      'error',
    );
  }
  memberStoryStore._loadItems();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  memberStoryStore.setListOptions(options); // Updated
};

const handleMemberStoryClosed = () => {
  closeAllDrawers();
};

const handleMemberStorySaved = () => {
  closeAllDrawers();
  memberStoryStore._loadItems();
};

const handleSearchUpdate = async (search: string) => {
  const processedSearch = removeDiacritics(search);
  searchQuery.value = search; // Giữ nguyên chuỗi tìm kiếm gốc cho hiển thị nếu cần
  memberStoryStore.setFilters({ searchQuery: processedSearch, memberId: props.memberId });
  memberStoryStore._loadItems();
};

// Watch for changes in memberId prop to update filters and reload items
watch(() => [props.memberId, props.familyId], ([newMemberId, newFamilyId]) => {
  memberStoryStore.setFilters({
    memberId: newMemberId || undefined,
    familyId: newFamilyId || undefined,
    searchQuery: searchQuery.value
  });
  memberStoryStore._loadItems();
}, { immediate: true });





</script>
