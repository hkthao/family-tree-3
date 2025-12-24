<template>
  <v-card :elevation="0" data-testid="member-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isAddingMember" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberForm ref="memberFormRef" @close="closeForm" :family-id="props.familyId" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm" :disabled="isAddingMember">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddMember" data-testid="save-member-button" :loading="isAddingMember" :disabled="isAddingMember">{{
        t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>
<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberForm } from '@/components/member';
import type { MemberAddDto } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useAddMemberMutation } from '@/composables';
interface MemberAddViewProps {
  familyId: string | null;
}
const props = defineProps<MemberAddViewProps>();
const emit = defineEmits(['close', 'saved']);
const memberFormRef = ref<InstanceType<typeof MemberForm> | null>(null);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const { mutate: addMember, isPending: isAddingMember } = useAddMemberMutation(); // Use mutation composable

const handleAddMember = async () => {
  if (!memberFormRef.value) return;
  const isValid = await memberFormRef.value.validate();
  if (!isValid) {
    return;
  }
  const memberData = memberFormRef.value.getFormData();
  if (props.familyId) {
    memberData.familyId = props.familyId;
  }

  addMember(memberData as MemberAddDto, {
    onSuccess: () => {
      showSnackbar(t('member.messages.addSuccess'), 'success');
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