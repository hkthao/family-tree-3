<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memberFace.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="queryLoading || isSubmitting" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberFaceForm ref="memberFaceFormRef" v-if="memberFace" :initial-member-face-data="memberFace" @close="closeForm" />
      <v-alert v-else type="info" class="mt-4">{{ t('memberFace.detail.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleSubmit" data-testid="save-member-face-button" :loading="isSubmitting">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberFaceForm } from '@/components/member-face';
import type { UpdateMemberFaceCommand } from '@/types';
import { useMemberFaceDetailQuery, useUpdateMemberFaceMutation } from '@/composables/member-face';
import { useMemberFaceFormLogic } from '@/composables/member-face/useMemberFaceFormLogic';

interface MemberFaceEditViewProps {
  memberFaceId: string;
}

const props = defineProps<MemberFaceEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const memberFaceFormRef = ref<InstanceType<typeof MemberFaceForm> | null>(null);

const { t } = useI18n();

const { memberFace, queryLoading } = useMemberFaceDetailQuery(props.memberFaceId);
const { mutateAsync: updateMemberFaceMutation } = useUpdateMemberFaceMutation();

const { isSubmitting, handleSubmit } = useMemberFaceFormLogic({
  mutation: updateMemberFaceMutation,
  successMessageKey: 'memberFace.messages.updateSuccess',
  errorMessageKey: 'memberFace.messages.saveError',
  formRef: memberFaceFormRef,
  onSuccess: () => {
    emit('saved');
  },
  transformData: (formData): UpdateMemberFaceCommand => {
    if (!memberFace.value?.id) {
      throw new Error(t('memberFace.messages.saveError'));
    }
    return {
      id: memberFace.value.id,
      memberId: formData.memberId,
      familyId: formData.familyId,
      // Only description can be updated via the form for now
      // Re-add other fields if the form allows updating them
    };
  },
  isUpdate: true,
});

const closeForm = () => {
  emit('close');
};
</script>