<template>
  <v-card :elevation="0" data-testid="member-face-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memberFace.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="add.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberFaceForm ref="memberFaceFormRef" @close="closeForm" :member-id="props.memberId" :family-id="props.familyId" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddMemberFace" data-testid="save-member-face-button" :loading="add.loading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberFaceStore } from '@/stores/member-face.store';
import { MemberFaceForm } from '@/components/member-face'; // Will be created later
import type { MemberFace } from '@/types';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

interface MemberFaceAddViewProps {
  memberId?: string;
  familyId?: string;
}

const props = defineProps<MemberFaceAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const memberFaceFormRef = ref<InstanceType<typeof MemberFaceForm> | null>(null);

const { t } = useI18n();
const memberFaceStore = useMemberFaceStore();
const { showSnackbar } = useGlobalSnackbar();

const { add } = storeToRefs(memberFaceStore);

const handleAddMemberFace = async () => {
  if (!memberFaceFormRef.value) return;
  const isValid = await memberFaceFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const memberFaceData = memberFaceFormRef.value.getFormData();
  // Ensure memberId and familyId are passed if they are props
  if (props.memberId) {
    memberFaceData.memberId = props.memberId;
  }
  if (props.familyId) {
    memberFaceData.familyId = props.familyId; // FamilyId on DTO is optional, but useful for creation context
  }

  try {
    await memberFaceStore.addItem(memberFaceData);
    if (!add.value.error && memberFaceStore.detail.item) {
      showSnackbar(t('memberFace.messages.addSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(add.value.error?.message || t('memberFace.messages.saveError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('memberFace.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>