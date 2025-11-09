<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberForm v-if="member" :initial-member-data="member" :family-id="member.familyId" :read-only="true" />
      <v-alert v-else type="info" class="mt-4">{{ t('member.detail.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!member || detail.loading">{{ t('common.edit') }}</v-btn>
      <v-btn color="info" @click="handleGenerateBiography" :disabled="!member || detail.loading">{{ t('ai.bioSuggest') }}</v-btn>
      <v-btn color="error" @click="handleDeleteFaceData" :disabled="!member || detail.loading">{{ t('face.deleteFaceData') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!member || detail.loading">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import { useFaceStore } from '@/stores/face.store';
import { useNotificationStore } from '@/stores/notification.store';
import { MemberForm } from '@/components/member';
import type { Member } from '@/types';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { storeToRefs } from 'pinia';

interface MemberDetailViewProps {
  memberId: string;
}

const props = defineProps<MemberDetailViewProps>();
const emit = defineEmits(['close', 'member-deleted', 'add-member-with-relationship', 'edit-member', 'generate-biography']);

const { t } = useI18n();
const memberStore = useMemberStore();
const faceStore = useFaceStore();
const notificationStore = useNotificationStore();
const { showConfirmDialog } = useConfirmDialog();

const { detail } = storeToRefs(memberStore);

const member = ref<Member | undefined>(undefined);

const loadMember = async (id: string) => {
  await memberStore.getById(id);
  if (memberStore.detail.item) {
    member.value = memberStore.detail.item;
  } else {
    member.value = undefined;
  }
};

onMounted(async () => {
  if (props.memberId) {
    await loadMember(props.memberId);
  }
});

watch(
  () => props.memberId,
  async (newId) => {
    if (newId) {
      await loadMember(newId);
    }
  },
);

const handleClose = () => {
  emit('close');
};

const handleEdit = () => {
  if (member.value) {
    emit('edit-member', member.value);
  }
};

const handleGenerateBiography = () => {
  if (member.value) {
    emit('generate-biography', member.value);
  }
};

const handleDelete = async () => {
  if (!member.value) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('member.list.confirmDelete', { fullName: member.value.fullName }),
  });

  if (confirmed) {
    try {
      await memberStore.deleteItem(member.value.id);
      if (!memberStore.error) {
        notificationStore.showSnackbar(t('member.messages.deleteSuccess'), 'success');
        emit('member-deleted'); // Notify parent that member was deleted
        emit('close'); // Close the detail drawer
      } else {
        notificationStore.showSnackbar(memberStore.error || t('member.messages.deleteError'), 'error');
      }
    } catch (error) {
      notificationStore.showSnackbar(t('member.messages.deleteError'), 'error');
    }
  }
};

const handleDeleteFaceData = async () => {
  if (!member.value) return;

  const confirmed = await showConfirmDialog({
    title: t('face.confirmDeleteFaceDataTitle'),
    message: t('face.confirmDeleteFaceDataMessage', { fullName: member.value.fullName }),
  });

  if (confirmed) {
    try {
      const result = await faceStore.deleteFacesByMemberId(member.value.id);
      if (result.ok) {
        notificationStore.showSnackbar(t('face.messages.deleteSuccess'), 'success');
      } else {
        notificationStore.showSnackbar(result.error?.message || t('face.messages.deleteError'), 'error');
      }
    } catch (error) {
      notificationStore.showSnackbar(t('face.messages.deleteError'), 'error');
    }
  }
};
</script>

<style scoped>
/* Add any specific styles for MemberDetailView here if needed */
</style>