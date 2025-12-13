<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLinkRequest.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLinkRequestForm v-if="familyLinkRequest" :initial-family-link-request-data="familyLinkRequest"
        :read-only="true" />
      <v-alert v-else type="info" class="mt-4">{{ t('familyLinkRequest.detail.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!familyLinkRequest || detail.loading"
        v-if="canEditOrDelete" data-testid="delete-button">{{ t('common.delete') }}</v-btn>
      <v-btn color="primary" @click="handleApprove"
        v-if="canApproveOrReject" data-testid="approve-button">
        {{ t('familyLinkRequest.list.action.approve') }}
      </v-btn>
      <v-btn color="warning" @click="handleReject"
        v-if="canApproveOrReject" data-testid="reject-button">
        {{ t('familyLinkRequest.list.action.reject') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyLinkRequestStore } from '@/stores/familyLinkRequest.store';
import { FamilyLinkRequestForm } from '@/components/family-link-requests';
import type { FamilyLinkRequestDto } from '@/types';
import { LinkStatus } from '@/types';
import { useConfirmDialog, useAuth, useGlobalSnackbar } from '@/composables';
import { storeToRefs } from 'pinia';

interface FamilyLinkRequestDetailViewProps {
  familyLinkRequestId: string;
  familyId: string;
}

const props = defineProps<FamilyLinkRequestDetailViewProps>();
const emit = defineEmits(['close', 'family-link-request-deleted', 'approve', 'reject']);

const { t } = useI18n();
const familyLinkRequestStore = useFamilyLinkRequestStore();
const { showConfirmDialog } = useConfirmDialog();
const { isAdmin, isFamilyManager } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const { detail } = storeToRefs(familyLinkRequestStore);

const familyLinkRequest = ref<FamilyLinkRequestDto | undefined>(undefined);

const canEditOrDelete = computed(() => {
  return isAdmin.value &&
    (familyLinkRequest.value?.requestingFamilyId === props.familyId ||
      familyLinkRequest.value?.targetFamilyId === props.familyId);
});

const canApproveOrReject = computed(() => {
  return familyLinkRequest.value && familyLinkRequest.value.status === LinkStatus.Pending && (
    (familyLinkRequest.value.targetFamilyId === props.familyId &&
    isFamilyManager.value(props.familyId)) || isAdmin.value);
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

const handleApprove = async () => {
  if (!familyLinkRequest.value) return;

  const confirmed = await showConfirmDialog({
    title: t('familyLinkRequest.list.confirmApprove.title'),
    message: t('familyLinkRequest.list.confirmApprove.message'),
    confirmText: t('familyLinkRequest.list.action.approve'),
    cancelText: t('common.cancel'),
    confirmColor: 'primary',
  });

  if (confirmed && props.familyId) {
    try {
      await familyLinkRequestStore.approveRequest(familyLinkRequest.value.id, props.familyId);
      if (!familyLinkRequestStore.error) {
        showSnackbar(t('familyLinkRequest.requests.messages.approveSuccess'), 'success');
        emit('approve', familyLinkRequest.value.id);
        emit('close');
      } else {
        showSnackbar(familyLinkRequestStore.error || t('familyLinkRequest.requests.messages.approveError'), 'error');
      }
    } catch (error) {
      showSnackbar(t('familyLinkRequest.requests.messages.approveError'), 'error');
    }
  }
};

const handleReject = async () => {
  if (!familyLinkRequest.value) return;

  const confirmed = await showConfirmDialog({
    title: t('familyLinkRequest.list.confirmReject.title'),
    message: t('familyLinkRequest.list.confirmReject.message'),
    confirmText: t('familyLinkRequest.list.action.reject'),
    cancelText: t('common.cancel'),
    confirmColor: 'warning',
  });

  if (confirmed && props.familyId) {
    try {
      await familyLinkRequestStore.rejectRequest(familyLinkRequest.value.id, props.familyId);
      if (!familyLinkRequestStore.error) {
        showSnackbar(t('familyLinkRequest.requests.messages.rejectSuccess'), 'success');
        emit('reject', familyLinkRequest.value.id);
        emit('close');
      } else {
        showSnackbar(familyLinkRequestStore.error || t('familyLinkRequest.requests.messages.rejectError'), 'error');
      }
    } catch (error) {
      showSnackbar(t('familyLinkRequest.requests.messages.rejectError'), 'error');
    }
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
