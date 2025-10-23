<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.form.editTitle') }}</span>
    </v-card-title>
    <v-card-text>
      <MemberForm
        ref="memberFormRef"
        v-if="member"
        :initial-member-data="member"
        @close="closeForm"
      />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey"  @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="blue-darken-1"  @click="handleUpdateMember">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import { MemberForm } from '@/components/members';
import type { Member } from '@/types';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const memberStore = useMemberStore();
const notificationStore = useNotificationStore();

const member = ref<Member | undefined>(undefined);
const memberFormRef = ref<InstanceType<typeof MemberForm> | null>(null);

onMounted(async () => {
  const memberId = route.params.id as string;
  if (memberId) {
    await memberStore.getById(memberId);
    member.value =  memberStore.currentItem as Member;
  }
});

const handleUpdateMember = async () => {
  if (!memberFormRef.value) return;
  const isValid = await memberFormRef.value.validate();
  if (!isValid) return;

  const memberData = memberFormRef.value.getFormData() as Member;
  if (!memberData.id) {
    notificationStore.showSnackbar(t('member.messages.saveError'), 'error');
    return;
  }

  try {
    await memberStore.updateItem(memberData as Member);
    if (!memberStore.error) {
      notificationStore.showSnackbar(t('member.messages.updateSuccess'), 'success');
      closeForm();
    } else {
      notificationStore.showSnackbar(memberStore.error || t('member.messages.saveError'), 'error');
    }
  } catch (error) {
    notificationStore.showSnackbar(t('member.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  router.push('/members');
};
</script>
