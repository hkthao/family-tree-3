<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memberFace.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <MemberFaceForm
        v-if="memberFace"
        :initial-member-face-data="memberFace"
        :read-only="true"
      />
      <v-alert v-else type="info" class="mt-4">{{ t('memberFace.detail.notFound') }}</v-alert>

      <!-- Display other details if necessary -->
      <v-divider class="my-4"></v-divider>
      <div v-if="memberFace">
        <v-list density="compact" class="pa-0">
          <v-list-item>
            <v-list-item-title>{{ t('memberFace.form.id') }}:</v-list-item-title>
            <v-list-item-subtitle class="text-wrap">{{ memberFace.id }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item>
            <v-list-item-title>{{ t('memberFace.form.faceId') }}:</v-list-item-title>
            <v-list-item-subtitle class="text-wrap">{{ memberFace.faceId }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item>
            <v-list-item-title>{{ t('memberFace.form.memberName') }}:</v-list-item-title>
            <v-list-item-subtitle>{{ memberFace.memberName }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item>
            <v-list-item-title>{{ t('memberFace.form.confidence') }}:</v-list-item-title>
            <v-list-item-subtitle>{{ memberFace.confidence?.toFixed(2) }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item v-if="memberFace.thumbnailUrl">
            <v-list-item-title>{{ t('memberFace.form.thumbnail') }}:</v-list-item-title>
            <v-list-item-subtitle>
              <v-img :src="memberFace.thumbnailUrl" max-width="100" class="my-2"></v-img>
            </v-list-item-subtitle>
          </v-list-item>
          <v-list-item v-if="memberFace.originalImageUrl">
            <v-list-item-title>{{ t('memberFace.form.originalImage') }}:</v-list-item-title>
            <v-list-item-subtitle>
              <a :href="memberFace.originalImageUrl" target="_blank">{{ t('memberFace.form.viewOriginal') }}</a>
            </v-list-item-subtitle>
          </v-list-item>
          <!-- Add more fields as needed -->
        </v-list>
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!memberFace || detail.loading">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!memberFace || detail.loading">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberFaceStore } from '@/stores/member-face.store';
import { MemberFaceForm } from '@/components/member-face'; // Will be created later
import type { MemberFace } from '@/types';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

interface MemberFaceDetailViewProps {
  memberFaceId: string;
}

const props = defineProps<MemberFaceDetailViewProps>();
const emit = defineEmits(['close', 'edit-member-face', 'member-face-deleted']);

const { t } = useI18n();
const memberFaceStore = useMemberFaceStore();
const { showSnackbar } = useGlobalSnackbar();
const { showConfirmDialog } = useConfirmDialog();

const { detail } = storeToRefs(memberFaceStore);

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

const handleClose = () => {
  emit('close');
};

const handleEdit = () => {
  if (memberFace.value) {
    emit('edit-member-face', memberFace.value.id);
  }
};

const handleDelete = async () => {
  if (!memberFace.value) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('memberFace.list.confirmDelete', { faceId: memberFace.value.faceId }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    try {
      await memberFaceStore.deleteItem(memberFace.value.id);
      if (!memberFaceStore.delete.error) {
        showSnackbar(t('memberFace.messages.deleteSuccess'), 'success');
        emit('member-face-deleted'); // Notify parent that member face was deleted
        emit('close'); // Close the detail drawer
      } else {
        showSnackbar(memberFaceStore.delete.error.message || t('memberFace.messages.deleteError'), 'error');
      }
    } catch (error) {
      showSnackbar(t('memberFace.messages.deleteError'), 'error');
    }
  }
};
</script>

