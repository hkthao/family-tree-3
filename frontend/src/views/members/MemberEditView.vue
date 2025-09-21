<template>
  <v-card>
    <v-card-text>
      <MemberForm
        v-if="member"
        :title="t('member.form.editTitle')"
        :initial-member-data="member"
        :members="members"
        :families="families"
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
import { useMembers } from '@/data/members';
import { useFamilies } from '@/data/families';
import { useNotificationStore } from '@/stores/notification';
import MemberForm from '@/components/members/MemberForm.vue';
import type { Member } from '@/types/member';
import type { Family } from '@/types/family';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const { getMemberById, updateMember, getMembers } = useMembers();
const { getFamilies } = useFamilies();
const notificationStore = useNotificationStore();

const member = ref<Member | undefined>(undefined);
const members = ref<Member[]>([]);
const families = ref<Family[]>([]);

onMounted(async () => {
  const memberId = route.params.id as string;
  member.value = getMemberById(memberId);

  const { members: fetchedMembers } = await getMembers({}, 1, -1); // Fetch all members
  members.value = fetchedMembers;

  const { families: fetchedFamilies } = await getFamilies('', 'All', 1, -1); // Fetch all families
  families.value = fetchedFamilies;
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
