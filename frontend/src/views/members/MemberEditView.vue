<template>
  <v-card>
    <v-card-text>
      <MemberForm
        v-if="initialMemberData"
        :initial-member-data="initialMemberData"
        :title="t('member.form.editTitle')"
        @close="closeForm"
        @submit="handleUpdateMember"
      />
      <v-progress-circular
        v-else
        indeterminate
        color="primary"
      ></v-progress-circular>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useMembers } from '@/data/members';
import { useNotificationStore } from '@/stores/notification';
import MemberForm from '@/components/members/MemberForm.vue';
import type { Member } from '@/types/member';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const { getMemberById, updateMember } = useMembers();
const notificationStore = useNotificationStore();

const initialMemberData = ref<Member | null>(null);

onMounted(() => {
  const memberId = route.params.id as string;
  if (memberId) {
    const member = getMemberById(memberId);
    if (member) {
      initialMemberData.value = {
        ...member,
        dateOfBirth: member.dateOfBirth ? new Date(member.dateOfBirth) : null,
        dateOfDeath: member.dateOfDeath ? new Date(member.dateOfDeath) : null,
        parents: [...member.parents],
        spouses: [...member.spouses],
        children: [...member.children]
      };
    } else {
      notificationStore.showSnackbar(t('member.messages.notFound'), 'error');
      router.push('/members');
    }
  } else {
    router.push('/members');
  }
});

const handleUpdateMember = async (memberData: Member) => {
  try {
    await updateMember(memberData);
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