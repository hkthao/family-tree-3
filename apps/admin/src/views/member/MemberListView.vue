<template>
  <div data-testid="member-list-view">
    <MemberSearch v-if="!props.hideSearch" @update:filters="handleFilterUpdate" />

    <MemberList :items="memberStore.list.items" :total-items="memberStore.list.totalItems" :loading="list.loading"
      :search="searchQuery" @update:search="handleSearchUpdate" @update:options="handleListOptionsUpdate"
      @view="openDetailDrawer" @edit="openEditDrawer" @delete="confirmDelete" @create="openAddDrawer()"
      @ai-biography="navigateToAIBiography" @ai-create="navigateToAICreateMember" :read-only="props.readOnly">
    </MemberList>

    <!-- Edit Member Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleMemberClosed">
      <MemberEditView v-if="selectedItemId && editDrawer" :member-id="selectedItemId"
        @close="handleMemberClosed" @saved="handleMemberSaved" />
    </BaseCrudDrawer>

    <!-- Add Member Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMemberClosed">
      <MemberAddView v-if="addDrawer" :family-id="props.familyId === undefined ? null : props.familyId"
        @close="handleMemberClosed" @saved="handleMemberSaved" />
    </BaseCrudDrawer>

    <!-- Detail Member Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <MemberDetailView v-if="selectedItemId && detailDrawer" :member-id="selectedItemId" @close="handleDetailClosed"
        @edit-member="openEditDrawer" @generate-biography="handleGenerateBiography" />
    </BaseCrudDrawer>

    <!-- Biography Drawer -->
    <BaseCrudDrawer v-model="biographyDrawer" @close="closeAllMemberDrawers">
      <MemberBiographyView v-if="biographyMemberId && biographyDrawer" :member-id="biographyMemberId"
        @close="closeAllMemberDrawers" />
    </BaseCrudDrawer>

    <!-- AI Create Member Drawer -->
    <BaseCrudDrawer v-model="aiCreateDrawer" @close="aiCreateDrawer = false">
      <NLEditorView v-if="aiCreateDrawer" :family-id="props.familyId || ''" @close="aiCreateDrawer = false" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { useMemberStore } from '@/stores/member.store';
import { MemberSearch, MemberList } from '@/components/member';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import MemberEditView from '@/views/member/MemberEditView.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import MemberDetailView from '@/views/member/MemberDetailView.vue';
import MemberBiographyView from '@/views/member/MemberBiographyView.vue';
import NLEditorView from '@/views/natural-language/NLEditorView.vue';
import type { MemberFilter, Member } from '@/types';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { onMounted, ref, watch } from 'vue';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // New import
import { useCrudDrawer } from '@/composables/useCrudDrawer'; // New import
import { removeDiacritics } from '@/utils/string.utils'; // NEW IMPORT

interface MemberListViewProps {
  familyId?: string;
  readOnly?: boolean;
  hideSearch?: boolean;
}

const props = defineProps<MemberListViewProps>();

const { t } = useI18n();
const memberStore = useMemberStore();
const { list } = storeToRefs(memberStore);
const searchQuery = ref('');

// Use the new composable
const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>(); // Specify the type for selectedItemId

const biographyDrawer = ref(false);
const biographyMemberId = ref<string | null>(null);
const aiCreateDrawer = ref(false);

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const navigateToAIBiography = (member: Member) => {
  handleGenerateBiography(member);
};

const navigateToAICreateMember = () => {
  aiCreateDrawer.value = true;
};

const handleFilterUpdate = async (filters: MemberFilter) => {
  memberStore.list.filters = { ...filters, searchQuery: searchQuery.value, familyId: props.familyId };
  await memberStore._loadItems()
};

const handleSearchUpdate = async (search: string) => {
  const processedSearch = removeDiacritics(search); // NEW: Preprocess search
  searchQuery.value = search; // Keep original search query for display if needed
  memberStore.list.filters = { ...memberStore.list.filters, searchQuery: processedSearch, familyId: props.familyId }; // Use processed search
  await memberStore._loadItems();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  memberStore.setListOptions(options);
};

const confirmDelete = async (memberId: string) => {
  const member = memberStore.list.items.find(m => m.id === memberId);
  if (!member) {
    showSnackbar(t('member.messages.notFound'), 'error');
    return;
  }
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('member.list.confirmDelete', { fullName: member.fullName || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    await handleDeleteConfirm(member);
  }
};

const handleDeleteConfirm = async (member: Member) => {
  if (member) {
    await memberStore.deleteItem(member.id);
    if (memberStore.error) {
      showSnackbar(
        t('member.messages.deleteError', { error: memberStore.error }),
        'error',
      );
    } else {
      showSnackbar(
        t('member.messages.deleteSuccess'),
        'success',
      );
    }
  }
  memberStore._loadItems();
};

const handleMemberSaved = () => {
  closeAllMemberDrawers(); // Close whichever drawer was open
  memberStore._loadItems();
};

const handleMemberClosed = () => {
  closeAllMemberDrawers(); // Close whichever drawer was open
};

const handleDetailClosed = () => {
  closeAllMemberDrawers(); // Close the detail drawer
};

const handleGenerateBiography = (member: Member) => {
  biographyMemberId.value = member.id;
  closeAllDrawers(); // Close detail drawer
  biographyDrawer.value = true;
};

const closeAllMemberDrawers = () => {
  closeAllDrawers();
  biographyDrawer.value = false;
  biographyMemberId.value = null;
  aiCreateDrawer.value = false;
};

onMounted(() => {
  memberStore.list.filters = { familyId: props.familyId };
  memberStore._loadItems();
})

watch(() => props.familyId, async (newFamilyId) => {
  memberStore.list.filters = { familyId: newFamilyId };
  await memberStore._loadItems();
}, { immediate: false });

</script>