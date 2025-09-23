<template>
  <v-card>
    <v-card-text>
      <MemberForm
        v-if="member"
        :title="t('member.form.editTitle')"
        :initial-member-data="member"
        @close="closeForm"
        @submit="handleUpdateMember"
      />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import MemberForm from '@/components/members/MemberForm.vue';
import type { Member } from '@/types/member';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const memberStore = useMemberStore();
const notificationStore = useNotificationStore();

const member = ref<Member | undefined>(undefined);

onMounted(() => {
  const memberId = route.params.id as string;
  member.value = memberStore.items.find(m => m.id === memberId);
});

const handleUpdateMember = async (memberData: Member) => {
  try {
    await memberStore.updateItem(memberData);
    notificationStore.showSnackbar(t('member.messages.updateSuccess'), 'success');
    closeForm();
  } catch (error) {
    notificationStore.showSnackbar(t('member.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  router.push('/members');
};
</script>
