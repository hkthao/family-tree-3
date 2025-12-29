<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyDict.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="loading || isDeletingFamilyDict" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <div v-if="loading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert type="error" :text="error?.message || t('familyDict.detail.notFound')"></v-alert>
      </div>
      <div v-else-if="familyDict">
        <PrivacyAlert :is-private="familyDict.isPrivate" />
        <FamilyDictForm :initial-family-dict-data="familyDict" :read-only="true" />
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose" :disabled="isDeletingFamilyDict">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!familyDict || loading || isDeletingFamilyDict"
        v-if="canEditOrDelete">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!familyDict || loading || isDeletingFamilyDict"
        v-if="canEditOrDelete">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyDictForm } from '@/components/family-dict';
import { useConfirmDialog, useAuth, useGlobalSnackbar } from '@/composables';
import { useFamilyDictQuery, useDeleteFamilyDictMutation } from '@/composables';
import PrivacyAlert from '@/components/common/PrivacyAlert.vue'; // Import PrivacyAlert

interface FamilyDictDetailViewProps {
  familyDictId: string;
}

const props = defineProps<FamilyDictDetailViewProps>();
const emit = defineEmits(['close', 'family-dict-deleted', 'edit-family-dict']);

const { t } = useI18n();
const { showConfirmDialog } = useConfirmDialog();
const { state } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const { familyDictId } = toRefs(props);
const { familyDict, loading, error } = useFamilyDictQuery(familyDictId);
const { mutate: deleteFamilyDict, isPending: isDeletingFamilyDict } = useDeleteFamilyDictMutation();

const canEditOrDelete = computed(() => {
  return state.isAdmin.value || state.isFamilyManager.value;
});

const handleClose = () => {
  emit('close');
};

const handleEdit = () => {
  if (familyDict.value) {
    emit('edit-family-dict', familyDict.value);
  }
};

const handleDelete = async () => {
  if (!familyDict.value) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyDict.list.confirmDelete', { name: familyDict.value.name }),
  });

  if (confirmed) {
    deleteFamilyDict(familyDict.value.id, {
      onSuccess: () => {
        showSnackbar(t('familyDict.messages.deleteSuccess'), 'success');
        emit('family-dict-deleted');
        emit('close');
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyDict.messages.deleteError'), 'error');
      },
    });
  }
};
</script>
