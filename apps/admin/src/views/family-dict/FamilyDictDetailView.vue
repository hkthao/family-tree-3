<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyDict.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyDictForm v-if="familyDict" :initial-family-dict-data="familyDict" :read-only="true" />
      <v-alert v-else type="info" class="mt-4">{{ t('familyDict.detail.notFound') }}</v-alert>

      <div v-if="familyDict" class="mt-4">
        <v-row>
          <v-col cols="12">
            <h3 class="text-h6">{{ t('familyDict.form.namesByRegion') }}</h3>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field :value="familyDict.namesByRegion.north" :label="t('familyDict.form.namesByRegion.north')"
              readonly></v-text-field>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field :value="familyDict.namesByRegion.central" :label="t('familyDict.form.namesByRegion.central')"
              readonly></v-text-field>
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field :value="familyDict.namesByRegion.south" :label="t('familyDict.form.namesByRegion.south')"
              readonly></v-text-field>
          </v-col>
        </v-row>
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!familyDict || detail.loading" v-if="canEditOrDelete">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!familyDict || detail.loading" v-if="canEditOrDelete">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyDictStore } from '@/stores/family-dict.store';
import { useNotificationStore } from '@/stores/notification.store';
import { FamilyDictForm } from '@/components/family-dict';
import type { FamilyDict } from '@/types';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { storeToRefs } from 'pinia';
import { useAuth } from '@/composables/useAuth';

interface FamilyDictDetailViewProps {
  familyDictId: string;
}

const props = defineProps<FamilyDictDetailViewProps>();
const emit = defineEmits(['close', 'family-dict-deleted', 'edit-family-dict']);

const { t } = useI18n();
const familyDictStore = useFamilyDictStore();
const notificationStore = useNotificationStore();
const { showConfirmDialog } = useConfirmDialog();
const { isAdmin, isFamilyManager } = useAuth();

const { detail } = storeToRefs(familyDictStore);

const familyDict = ref<FamilyDict | undefined>(undefined);

const canEditOrDelete = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

const loadFamilyDict = async (id: string) => {
  await familyDictStore.getById(id);
  if (familyDictStore.detail.item) {
    familyDict.value = familyDictStore.detail.item;
  } else {
    familyDict.value = undefined;
  }
};

onMounted(async () => {
  if (props.familyDictId) {
    await loadFamilyDict(props.familyDictId);
  }
});

watch(
  () => props.familyDictId,
  async (newId) => {
    if (newId) {
      await loadFamilyDict(newId);
    }
  },
);

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
    try {
      await familyDictStore.deleteItem(familyDict.value.id);
      if (!familyDictStore.error) {
        notificationStore.showSnackbar(t('familyDict.messages.deleteSuccess'), 'success');
        emit('family-dict-deleted');
        emit('close');
      } else {
        notificationStore.showSnackbar(familyDictStore.error || t('familyDict.messages.deleteError'), 'error');
      }
    } catch (error) {
      notificationStore.showSnackbar(t('familyDict.messages.deleteError'), 'error');
    }
  }
};
</script>
