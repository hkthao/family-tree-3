<template>
  <div data-testid="member-list-view">
    <MemberSearch v-if="!props.hideSearch" @update:filters="handleFilterUpdate" />

    <MemberList :items="memberStore.list.items" :total-items="memberStore.list.totalItems" :loading="list.loading"
      :search="searchQuery" @update:search="handleSearchUpdate" @update:options="handleListOptionsUpdate"
      @view="navigateToDetailView" @edit="navigateToEditMember" @delete="confirmDelete" @create="navigateToCreateView()"
      @ai-biography="navigateToAIBiography" @ai-create="navigateToAICreateMember" :read-only="props.readOnly">
    </MemberList>
      
    <!-- Edit Member Drawer -->

    <v-navigation-drawer v-model="editDrawer" location="right" temporary width="650">
      <MemberEditView v-if="selectedMemberId && editDrawer" :member-id="selectedMemberId" @close="handleMemberClosed"
        @saved="handleMemberSaved" />
    </v-navigation-drawer>

    <!-- Add Member Drawer -->
    <v-navigation-drawer v-model="addDrawer" location="right" temporary width="650">
      <MemberAddView v-if="addDrawer" :family-id="props.familyId === undefined ? null : props.familyId"
        @close="handleMemberClosed" @saved="handleMemberSaved" />
    </v-navigation-drawer>

    <!-- Detail Member Drawer -->
    <v-navigation-drawer v-model="detailDrawer" location="right" temporary width="650">
      <MemberDetailView v-if="selectedMemberId && detailDrawer" :member-id="selectedMemberId"
        @close="handleDetailClosed" @edit-member="navigateToEditMember" @generate-biography="handleGenerateBiography" />
    </v-navigation-drawer>

    <!-- Biography Drawer -->
    <v-navigation-drawer v-model="biographyDrawer" location="right" temporary width="650">
      <MemberBiographyView v-if="biographyMemberId && biographyDrawer" :member-id="biographyMemberId"
        @close="handleBiographyClosed" />
    </v-navigation-drawer>

    <!-- AI Create Member Drawer -->
    <v-navigation-drawer v-model="aiCreateDrawer" location="right" temporary width="650">
      <NLEditorView v-if="aiCreateDrawer" :family-id="props.familyId || ''" @close="aiCreateDrawer = false" />
    </v-navigation-drawer>
  </div>
</template>

<script setup lang="ts">
import { useMemberStore } from '@/stores/member.store';
import { MemberSearch, MemberList } from '@/components/member';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useNotificationStore } from '@/stores/notification.store';
import MemberEditView from '@/views/member/MemberEditView.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import MemberDetailView from '@/views/member/MemberDetailView.vue';
import MemberBiographyView from '@/views/member/MemberBiographyView.vue';
import NLEditorView from '@/views/natural-language/NLEditorView.vue';
import type { MemberFilter, Member } from '@/types';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { onMounted, ref } from 'vue';

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
const editDrawer = ref(false);
const addDrawer = ref(false);
const selectedMemberId = ref<string | null>(null);
const detailDrawer = ref(false);
const biographyDrawer = ref(false);
const biographyMemberId = ref<string | null>(null);
const initialMemberData = ref<Member | null>(null);
const aiCreateDrawer = ref(false);

const notificationStore = useNotificationStore();
const { showConfirmDialog } = useConfirmDialog();

const navigateToDetailView = (member: Member) => {
  selectedMemberId.value = member.id;
  detailDrawer.value = true;
};

const navigateToCreateView = (initialData: Member | null = null) => {
  initialMemberData.value = initialData;
  addDrawer.value = true;
};

const navigateToEditMember = (member: Member) => {
  selectedMemberId.value = member.id;
  detailDrawer.value = false;
  editDrawer.value = true;
};

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
  searchQuery.value = search;
  memberStore.list.filters = { ...memberStore.list.filters, searchQuery: searchQuery.value, familyId: props.familyId };
  await memberStore._loadItems();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  memberStore.setListOptions(options);
};

const confirmDelete = async (member: Member) => {
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
  memberStore._loadItems();
};

const handleMemberSaved = () => {
  editDrawer.value = false;
  addDrawer.value = false;
  selectedMemberId.value = null;
  initialMemberData.value = null;
  memberStore._loadItems();
};

const handleMemberClosed = () => {
  editDrawer.value = false;
  addDrawer.value = false;
  selectedMemberId.value = null;
  initialMemberData.value = null;
};

const handleDetailClosed = () => {
  detailDrawer.value = false;
  selectedMemberId.value = null;
};

const handleGenerateBiography = (member: Member) => {
  biographyMemberId.value = member.id;
  detailDrawer.value = false;
  biographyDrawer.value = true;
};

const handleBiographyClosed = () => {
  biographyDrawer.value = false;
  biographyMemberId.value = null;
};

onMounted(() => {
  if (props.familyId) {
    memberStore.getByFamilyId(props.familyId);
  } else {
    memberStore._loadItems();
  }
})

</script>