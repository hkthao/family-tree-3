<template>
  <v-container fluid>
    <v-card class="mb-4">
      <v-card-title class="d-flex align-center text-uppercase">
        <span class="text-h5 text-uppercase">{{ t('member.detail.title') }}</span>
        <v-spacer></v-spacer>
        <v-btn color="primary" icon @click="navigateToEditMember">
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
        <v-btn color="error" icon @click="confirmDelete">
          <v-icon>mdi-delete</v-icon>
        </v-btn>
        <v-btn icon @click="closeDetail">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-card-title>
    </v-card>

    <MemberForm
      v-if="member"
      :member="member"
      readOnly
    />
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useMembers } from '@/data/members';
import type { Member } from '@/types/member';
import MemberForm from '@/components/members/MemberForm.vue';
import { useNotificationStore } from '@/stores/notification';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const { getMemberById } = useMembers();
const notificationStore = useNotificationStore();

const member = ref<Member | undefined>(undefined);
const deleteConfirmDialog = ref(false);
const memberToDelete = ref<Member | undefined>(undefined);

const loadMember = async () => {
  const memberId = route.params.id as string;
  const fetchedMember = await getMemberById(memberId);
  if (fetchedMember) {
    member.value = fetchedMember;
  } else {
    notificationStore.showSnackbar(t('member.messages.notFound'), 'error');
    router.push('/members');
  }
};

const navigateToEditMember = () => {
  router.push(`/members/edit/${member.value?.id}`);
};

const confirmDelete = () => {
  memberToDelete.value = member.value;
  deleteConfirmDialog.value = true;
};

const closeDetail = () => {
  router.push('/members');
};

onMounted(() => {
  loadMember();
});
</script>