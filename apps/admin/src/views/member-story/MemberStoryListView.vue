<template>
  <div>
    <MemberStoryList
      :items="items"
      :total-items="totalItems"
      :loading="loading"
      :items-per-page="(listOptions.itemsPerPage as number)"
      :search="searchQuery"
      @update:options="handleListOptionsUpdate"
      @update:search="handleSearchUpdate"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      @create="openAddDrawer"
    />

    <v-alert v-if="isListError" type="error" dismissible class="mt-4">
      {{ listError?.message || t('memberStory.list.loadError') }}
    </v-alert>

    <!-- Add MemberStory Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMemberStoryClosed">
      <MemberStoryAddView v-if="addDrawer" @close="handleMemberStoryClosed" @saved="handleMemberStorySaved" :family-id="props.familyId" />
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
import { watch, ref, reactive, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useCrudDrawer, useConfirmDialog, useGlobalSnackbar, useAuth } from '@/composables';
import { useMemberStoriesQuery, useDeleteMemberStoryMutation } from '@/composables/memberStory';
import type { MemberStoryListOptions } from '@/composables/memberStory/useMemberStoriesQuery'; // Import interface
import type { MemberStoryDto } from '@/types/memberStory';
import MemberStoryAddView from './MemberStoryAddView.vue';
import MemberStoryEditView from './MemberStoryEditView.vue';
import MemberStoryDetailView from './MemberStoryDetailView.vue';
import MemberStoryList from '@/components/member-story/MemberStoryList.vue';
import { removeDiacritics } from '@/utils/string.utils';

interface MemberStoryListViewProps {
  memberId?: string;
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<MemberStoryListViewProps>();

const { t } = useI18n();
const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const searchQuery = ref(''); // Use a ref to hold the current search query for filtering
useAuth();

const listOptions = reactive<MemberStoryListOptions>({
  page: 1,
  itemsPerPage: 10,
  sortBy: [],
  searchQuery: '',
  memberId: props.memberId,
  familyId: props.familyId,
});

const { data: memberStoriesData, isLoading: isListLoading, isError: isListError, error: listError } = useMemberStoriesQuery(listOptions);
const { mutateAsync: deleteMemberStory, isPending: isDeleting } = useDeleteMemberStoryMutation();

const items = computed(() => memberStoriesData.value?.items || []);
const totalItems = computed(() => memberStoriesData.value?.totalItems || 0);
const loading = computed(() => isListLoading.value || isDeleting.value);

watch([() => props.memberId, () => props.familyId], ([newMemberId, newFamilyId]) => {
  listOptions.memberId = newMemberId;
  listOptions.familyId = newFamilyId;
}, { immediate: true });

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
    try {
      await deleteMemberStory(item.id);
      showSnackbar(t('memberStory.messages.deleteSuccess'), 'success');
    } catch (error) {
      showSnackbar((error as Error).message, 'error');
    }
  } else {
    showSnackbar(t('memberStory.messages.deleteError', { error: t('common.invalidId') }), 'error');
  }
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  listOptions.page = options.page;
  listOptions.itemsPerPage = options.itemsPerPage;
  listOptions.sortBy = options.sortBy;
};

const handleMemberStoryClosed = () => {
  closeAllDrawers();
};

const handleMemberStorySaved = () => {
  closeAllDrawers();
};

const handleSearchUpdate = async (search: string) => {
  const processedSearch = removeDiacritics(search);
  searchQuery.value = search;
  listOptions.searchQuery = processedSearch;
};
</script>