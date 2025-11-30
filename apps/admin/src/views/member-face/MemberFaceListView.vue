<template>
  <div data-testid="member-face-list-view">
    <MemberFaceList
      :items="memberFaceStore.list.items"
      :total-items="memberFaceStore.list.totalItems"
      :loading="list.loading"
      @update:options="handleListOptionsUpdate"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      @create="openAddDrawer()"
    ></MemberFaceList>

    <!-- Edit MemberFace Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleMemberFaceClosed">
      <MemberFaceEditView
        v-if="selectedItemId && editDrawer"
        :member-face-id="selectedItemId"
        @close="handleMemberFaceClosed"
        @saved="handleMemberFaceSaved"
      />
    </BaseCrudDrawer>

    <!-- Add MemberFace Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMemberFaceClosed">
      <MemberFaceAddView
        v-if="addDrawer"
        :member-id="props.memberId"
        :family-id="props.familyId"
        @close="handleMemberFaceClosed"
        @saved="handleMemberFaceSaved"
      />
    </BaseCrudDrawer>

    <!-- Detail MemberFace Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <MemberFaceDetailView
        v-if="selectedItemId && detailDrawer"
        :member-face-id="selectedItemId"
        @close="handleDetailClosed"
        @edit-member-face="openEditDrawer"
      />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useCrudDrawer } from '@/composables/useCrudDrawer';
import { useMemberFaceStore } from '@/stores/member-face.store';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import type { MemberFace } from '@/types'; // Added MemberFace import

// Components (will be created later)
import MemberFaceList from '@/components/member-face/MemberFaceList.vue';
import MemberFaceAddView from '@/views/member-face/MemberFaceAddView.vue';
import MemberFaceEditView from '@/views/member-face/MemberFaceEditView.vue';
import MemberFaceDetailView from '@/views/member-face/MemberFaceDetailView.vue';

interface MemberFaceListViewProps {
  memberId?: string;
  familyId?: string;
}

const props = defineProps<MemberFaceListViewProps>();

const { t } = useI18n();
const memberFaceStore = useMemberFaceStore();
const { list } = storeToRefs(memberFaceStore);
const { showSnackbar } = useGlobalSnackbar();
const { showConfirmDialog } = useConfirmDialog();

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

onMounted(() => {
  memberFaceStore.list.filters = {
    memberId: props.memberId,
    familyId: props.familyId,
  };
  memberFaceStore._loadItems();
});

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  memberFaceStore.setListOptions(options);
  memberFaceStore._loadItems();
};

const confirmDelete = async (memberFace: MemberFace) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('memberFace.list.confirmDelete', { faceId: memberFace.faceId }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    await handleDeleteConfirm(memberFace);
  }
};

const handleDeleteConfirm = async (memberFace: MemberFace) => {
  if (memberFace) {
    await memberFaceStore.deleteItem(memberFace.id);
    if (memberFaceStore.delete.error) {
      showSnackbar(
        memberFaceStore.delete.error.message || t('memberFace.messages.deleteError'),
        'error',
      );
    } else {
      showSnackbar(t('memberFace.messages.deleteSuccess'), 'success');
    }
  }
  memberFaceStore._loadItems();
};

const handleMemberFaceSaved = () => {
  closeAllDrawers();
  memberFaceStore._loadItems();
};

const handleMemberFaceClosed = () => {
  closeAllDrawers();
};

const handleDetailClosed = () => {
  closeAllDrawers();
};
</script>
