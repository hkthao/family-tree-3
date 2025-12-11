<template>
  <v-card :elevation="0" data-testid="member-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="add.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberForm ref="memberFormRef" @close="closeForm" :family-id="props.familyId" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddMember" data-testid="save-member-button" :loading="add.loading">{{
        t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>
<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import { MemberForm } from '@/components/member';
import type { Member } from '@/types';
import { v4 as uuidv4 } from 'uuid';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables'; // Import useGlobalSnackbar
interface MemberAddViewProps {
  familyId: string | null;
}
const props = defineProps<MemberAddViewProps>();
const emit = defineEmits(['close', 'saved']);
const memberFormRef = ref<InstanceType<typeof MemberForm> | null>(null);
const { t } = useI18n();
const memberStore = useMemberStore();
const { showSnackbar } = useGlobalSnackbar(); // Khởi tạo useGlobalSnackbar
const { add } = storeToRefs(memberStore);
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
  try {
    const newMember: Member = {
      ...memberData,
      id: uuidv4(),
    };
    await memberStore.addItem(newMember);
    if (!memberStore.error) {
      showSnackbar(t('member.messages.addSuccess'), 'success');
      emit('saved'); // Emit saved event
    } else {
      showSnackbar(memberStore.error || t('member.messages.saveError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('member.messages.saveError'), 'error');
  }
};
const closeForm = () => {
  emit('close'); // Emit close event
};
</script>