<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingMember || isUpdatingMember" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberForm ref="memberFormRef" v-if="member" :initial-member-data="member" @close="closeForm" :family-id="member.familyId" />
      <v-alert v-else-if="memberError" type="error" class="mt-4">{{ memberError.message || t('member.messages.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm" :disabled="isLoadingMember || isUpdatingMember">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateMember" data-testid="save-member-button" :loading="isUpdatingMember" :disabled="isLoadingMember || isUpdatingMember">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberForm } from '@/components/member';
import type { Member } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useMemberQuery, useUpdateMemberMutation } from '@/composables/member'; // Import new composables

interface MemberEditViewProps {
  memberId: string;
}

const props = defineProps<MemberEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const memberFormRef = ref<InstanceType<typeof MemberForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const memberIdRef = toRef(props, 'memberId');
const { data: member, isLoading: isLoadingMember, error: memberError } = useMemberQuery(memberIdRef);
const { mutate: updateMember, isPending: isUpdatingMember } = useUpdateMemberMutation();

const handleUpdateMember = async () => {
  if (!memberFormRef.value) return;
  const isValid = await memberFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const memberData = memberFormRef.value.getFormData() as Member;
  if (!memberData.id) {
    showSnackbar(t('member.messages.saveError'), 'error');
    return;
  }

  updateMember(memberData, {
    onSuccess: () => {
      showSnackbar(t('member.messages.updateSuccess'), 'success');
      emit('saved');
    },
    onError: (error) => {
      showSnackbar(error.message || t('member.messages.saveError'), 'error');
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>
