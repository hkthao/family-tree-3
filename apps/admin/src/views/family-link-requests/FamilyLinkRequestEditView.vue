<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLinkRequest.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading || update.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLinkRequestForm ref="familyLinkRequestFormRef" v-if="familyLinkRequest" :initial-family-link-request-data="familyLinkRequest" :read-only="false" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateFamilyLinkRequest" data-testid="save-family-link-request-button" :loading="update.loading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyLinkRequestStore } from '@/stores/familyLinkRequest.store';
import { FamilyLinkRequestForm } from '@/components/family-link-requests';
import type { FamilyLinkRequestDto, UpdateFamilyLinkRequestCommand } from '@/types';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

interface FamilyLinkRequestEditViewProps {
  familyLinkRequestId: string;
  familyId: string;
}

const props = defineProps<FamilyLinkRequestEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyLinkRequestFormRef = ref<InstanceType<typeof FamilyLinkRequestForm> | null>(null);

const { t } = useI18n();
const familyLinkRequestStore = useFamilyLinkRequestStore();
const { showSnackbar } = useGlobalSnackbar();

const { detail, update } = storeToRefs(familyLinkRequestStore);

const familyLinkRequest = ref<FamilyLinkRequestDto | undefined>(undefined);

const loadFamilyLinkRequest = async (id: string) => {
  await familyLinkRequestStore.getById(id);
  if (familyLinkRequestStore.detail.item)
    familyLinkRequest.value = familyLinkRequestStore.detail.item;
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

const handleUpdateFamilyLinkRequest = async () => {
  if (!familyLinkRequestFormRef.value || !familyLinkRequest.value) return;
  const isValid = await familyLinkRequestFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const formData = familyLinkRequestFormRef.value.getFormData();
  const updateCommand: UpdateFamilyLinkRequestCommand = {
    id: familyLinkRequest.value.id,
    status: formData.status!,
  };

  try {
    const result = await familyLinkRequestStore.updateRequest(updateCommand, props.familyId);
    if (result.ok) {
      showSnackbar(t('familyLinkRequest.requests.messages.updateSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(result.error?.message || t('familyLinkRequest.requests.messages.updateError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('familyLinkRequest.requests.messages.updateError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>