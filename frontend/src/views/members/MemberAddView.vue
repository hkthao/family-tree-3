<template>
  <v-card>
    <v-card-text>
      <MemberForm
        :title="t('member.form.addTitle')"
        @close="closeForm"
        @submit="handleAddMember"
      />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import MemberForm from '@/components/members/MemberForm.vue';
import type { Member } from '@/types/member';

const { t } = useI18n();
const router = useRouter();
const memberStore = useMemberStore();
const notificationStore = useNotificationStore();

const handleAddMember = async (memberData: Omit<Member, 'id'>) => {
  try {
    await memberStore.addItem(memberData);
    notificationStore.showSnackbar(t('member.messages.addSuccess'), 'success');
    closeForm();
  } catch (error) {
    notificationStore.showSnackbar(t('member.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  router.push('/members');
};
</script>