<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memberFace.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="queryLoading || isUpdating" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberFaceForm ref="memberFaceFormRef" v-if="memberFace" :initial-member-face-data="memberFace" @close="closeForm" />
      <v-alert v-else type="info" class="mt-4">{{ t('memberFace.detail.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateMemberFace" data-testid="save-member-face-button" :loading="isUpdating">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberFaceForm } from '@/components/member-face';
import type { MemberFace } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useMemberFaceDetailQuery, useUpdateMemberFaceMutation } from '@/composables/member-face';

interface MemberFaceEditViewProps {
  memberFaceId: string;
}

const props = defineProps<MemberFaceEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const memberFaceFormRef = ref<InstanceType<typeof MemberFaceForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { memberFace, queryLoading } = useMemberFaceDetailQuery(props.memberFaceId);
const { mutate: updateMemberFace, isPending: isUpdating } = useUpdateMemberFaceMutation();

const handleUpdateMemberFace = async () => {
  if (!memberFaceFormRef.value) return;
  const isValid = await memberFaceFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const memberFaceData = memberFaceFormRef.value.getFormData() as MemberFace;
  if (!memberFaceData.id) {
    showSnackbar(t('memberFace.messages.saveError'), 'error');
    return;
  }

  updateMemberFace(memberFaceData, {
    onSuccess: () => {
      showSnackbar(t('memberFace.messages.updateSuccess'), 'success');
      emit('saved');
    },
    onError: (error) => {
      showSnackbar(error.message || t('memberFace.messages.saveError'), 'error');
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>