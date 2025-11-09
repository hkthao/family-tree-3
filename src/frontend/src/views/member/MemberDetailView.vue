<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <MemberForm v-if="member" :initial-member-data="member" :family-id="member.familyId" :read-only="true" />
      <v-alert v-else type="info" class="mt-4">{{ t('member.detail.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!member">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!member">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import { MemberForm } from '@/components/member';
import type { Member } from '@/types';
import { useConfirmDialog } from '@/composables/useConfirmDialog';

interface MemberDetailViewProps {
  memberId: string;
}

const props = defineProps<MemberDetailViewProps>();
const emit = defineEmits(['close', 'member-deleted', 'add-member-with-relationship', 'edit-member']);

const { t } = useI18n();
const router = useRouter();
const memberStore = useMemberStore();
const notificationStore = useNotificationStore();
const { showConfirmDialog } = useConfirmDialog();

const member = ref<Member | undefined>(undefined);

const loadMember = async (id: string) => {
  await memberStore.getById(id);
  if (memberStore.currentItem) {
    member.value = memberStore.currentItem;
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

const handleDelete = async () => {
  if (!member.value) return;

  const confirmed = await showConfirmDialog(
    t('confirmDelete.title'),
    t('member.list.confirmDelete', { fullName: member.value.fullName })
  );

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
</script>

<style scoped>
/* Add any specific styles for MemberDetailView here if needed */
</style>