<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLinkRequest.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLinkRequestForm v-if="familyLinkRequest" :initial-family-link-request-data="familyLinkRequest" :read-only="true" />
      <v-list v-if="familyLinkRequest" density="compact" class="mt-4">
        <v-list-item v-if="familyLinkRequest.requestMessage">
          <v-list-item-title class="font-weight-bold">{{ t('familyLinkRequest.form.requestMessage') }}:</v-list-item-title>
          <v-list-item-subtitle>{{ familyLinkRequest.requestMessage }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item v-if="familyLinkRequest.responseMessage">
          <v-list-item-title class="font-weight-bold">{{ t('familyLinkRequest.form.responseMessage') }}:</v-list-item-title>
          <v-list-item-subtitle>{{ familyLinkRequest.responseMessage }}</v-list-item-subtitle>
        </v-list-item>
      </v-list>
      <v-alert v-else type="info" class="mt-4">{{ t('familyLinkRequest.detail.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!familyLinkRequest || detail.loading" v-if="canEditOrDelete">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!familyLinkRequest || detail.loading" v-if="canEditOrDelete">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyLinkRequestStore } from '@/stores/familyLinkRequest.store';
import { FamilyLinkRequestForm } from '@/components/family-link-requests';
import type { FamilyLinkRequestDto } from '@/types';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { storeToRefs } from 'pinia';
import { useAuth } from '@/composables/useAuth';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

interface FamilyLinkRequestDetailViewProps {
  familyLinkRequestId: string;
  familyId: string;
}

const props = defineProps<FamilyLinkRequestDetailViewProps>();
const emit = defineEmits(['close', 'family-link-request-deleted', 'edit-family-link-request']);

const { t } = useI18n();
const familyLinkRequestStore = useFamilyLinkRequestStore();
const { showConfirmDialog } = useConfirmDialog();
const { isAdmin, isFamilyManager } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const { detail } = storeToRefs(familyLinkRequestStore);

const familyLinkRequest = ref<FamilyLinkRequestDto | undefined>(undefined);

const canEditOrDelete = computed(() => {
  // User must be admin of requesting or target family to edit/delete
  return (isAdmin.value || isFamilyManager.value(props.familyId)) && 
         (familyLinkRequest.value?.requestingFamilyId === props.familyId || 
          familyLinkRequest.value?.targetFamilyId === props.familyId);
});

const loadFamilyLinkRequest = async (id: string) => {
  await familyLinkRequestStore.getById(id);
  if (familyLinkRequestStore.detail.item) {
    familyLinkRequest.value = familyLinkRequestStore.detail.item;
  } else {
    familyLinkRequest.value = undefined;
  }
};

onMounted(async () => {
  if (props.familyLinkRequestId) {
    await loadFamilyLinkRequest(props.familyLinkRequestId);
  }
});

watch(
  () => props.familyLinkRequestId,
  async (newId) => {
    if (newId) {
      await loadFamilyLinkRequest(newId);
    }
  },
);

const handleClose = () => {
  emit('close');
};

const handleEdit = () => {
  if (familyLinkRequest.value) {
    emit('edit-family-link-request', familyLinkRequest.value.id);
  }
};

const handleDelete = async () => {
  if (!familyLinkRequest.value) return;

  const confirmed = await showConfirmDialog({
    title: t('familyLinkRequest.list.confirmDelete.title'),
    message: t('familyLinkRequest.list.confirmDelete.message'),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed && props.familyId) {
    try {
      await familyLinkRequestStore.deleteRequest(familyLinkRequest.value.id, props.familyId);
      if (!familyLinkRequestStore.error) {
        showSnackbar(t('familyLinkRequest.list.messages.deleteSuccess'), 'success');
        emit('family-link-request-deleted');
        emit('close');
      } else {
        showSnackbar(familyLinkRequestStore.error || t('familyLinkRequest.list.messages.deleteError'), 'error');
      }
    } catch (error) {
      showSnackbar(t('familyLinkRequest.list.messages.deleteError'), 'error');
    }
  }
};
</script>
