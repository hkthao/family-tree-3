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
import { useMembers } from '@/data/members';
import { useNotificationStore } from '@/stores/notification';
import MemberForm from '@/components/members/MemberForm.vue';
import type { Member } from '@/types/member';

const { t } = useI18n();
const router = useRouter();
const { addMember } = useMembers();
const notificationStore = useNotificationStore();

const handleAddMember = async (memberData: Omit<Member, 'id'>) => {
  try {
    await addMember(memberData);
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