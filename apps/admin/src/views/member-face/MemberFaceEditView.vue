<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memberFace.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading || update.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberFaceForm ref="memberFaceFormRef" v-if="memberFace" :initial-member-face-data="memberFace" @close="closeForm" />
      <v-alert v-else type="info" class="mt-4">{{ t('memberFace.detail.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateMemberFace" data-testid="save-member-face-button" :loading="update.loading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberFaceStore } from '@/stores/member-face.store';
import { MemberFaceForm } from '@/components/member-face'; // Will be created later
import type { MemberFace } from '@/types';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables';

interface MemberFaceEditViewProps {
  memberFaceId: string;
}

const props = defineProps<MemberFaceEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const memberFaceFormRef = ref<InstanceType<typeof MemberFaceForm> | null>(null);

const { t } = useI18n();
const memberFaceStore = useMemberFaceStore();
const { showSnackbar } = useGlobalSnackbar();

const { detail, update } = storeToRefs(memberFaceStore);

const memberFace = ref<MemberFace | undefined>(undefined);

const loadMemberFace = async (id: string) => {
  await memberFaceStore.getById(id);
  if (memberFaceStore.detail.item) {
    memberFace.value = memberFaceStore.detail.item;
  } else {
    memberFace.value = undefined;
  }
};

onMounted(async () => {
  if (props.memberFaceId) {
    await loadMemberFace(props.memberFaceId);
  }
});

watch(
  () => props.memberFaceId,
  async (newId) => {
    if (newId) {
      await loadMemberFace(newId);
    }
  },
);

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

  try {
    await memberFaceStore.updateItem(memberFaceData);
    if (!update.value.error) {
      showSnackbar(t('memberFace.messages.updateSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(update.value.error?.message || t('memberFace.messages.saveError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('memberFace.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>