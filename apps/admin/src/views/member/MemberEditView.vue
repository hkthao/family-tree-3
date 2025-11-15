<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading || update.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberForm ref="memberFormRef" v-if="member" :initial-member-data="member" @close="closeForm" :family-id="member.familyId" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateMember" data-testid="save-member-button" :loading="update.loading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import { MemberForm } from '@/components/member';
import type { Member } from '@/types';
import { storeToRefs } from 'pinia';

interface MemberEditViewProps {
  memberId: string;
}

const props = defineProps<MemberEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const memberFormRef = ref<InstanceType<typeof MemberForm> | null>(null);

const { t } = useI18n();
const memberStore = useMemberStore();
const notificationStore = useNotificationStore();

const { detail, update } = storeToRefs(memberStore);

const member = ref<Member | undefined>(undefined);

const loadMember = async (id: string) => {
  await memberStore.getById(id);
  if (memberStore.detail.item)
    member.value = memberStore.detail.item;
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

const handleUpdateMember = async () => {
  if (!memberFormRef.value) return;
  const isValid = await memberFormRef.value.validate();
  console.log('Validation result:', isValid);
  if (!isValid) {
    console.log('Validation errors:', memberFormRef.value.v$.$errors);
    return;
  }

  const memberData = memberFormRef.value.getFormData() as Member;
  if (!memberData.id) { // Use memberData.id for the check
    notificationStore.showSnackbar(t('member.messages.saveError'), 'error');
    return;
  }

  try {
    await memberStore.updateItem(memberData as Member);
    if (!memberStore.error) {
      notificationStore.showSnackbar(t('member.messages.updateSuccess'), 'success');
      emit('saved'); // Emit saved event
    } else {
      notificationStore.showSnackbar(memberStore.error || t('member.messages.saveError'), 'error');
    }
  } catch (error) {
    notificationStore.showSnackbar(t('member.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close'); // Emit close event
};
</script>
