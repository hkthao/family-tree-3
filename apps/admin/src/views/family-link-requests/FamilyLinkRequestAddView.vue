<template>
  <v-card :elevation="0" data-testid="family-link-request-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLinkRequest.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="add.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLinkRequestForm
        ref="familyLinkRequestFormRef"
        :read-only="false"
        :initial-family-link-request-data="{ requestingFamilyId: familyId } as Partial<FamilyLinkRequestDto>"
      />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn
        color="primary"
        @click="handleAddFamilyLinkRequest"
        data-testid="save-family-link-request-button"
        :loading="add.loading"
      >{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyLinkRequestStore } from '@/stores/familyLinkRequest.store';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import FamilyLinkRequestForm from '@/components/family-link-requests/FamilyLinkRequestForm.vue';
import type { FamilyLinkRequestDto } from '@/types';

const emit = defineEmits(['close', 'saved']);

const { familyId } = defineProps<{
  familyId: string;
}>();

const familyLinkRequestFormRef = ref<InstanceType<typeof FamilyLinkRequestForm> | null>(null);

const { t } = useI18n();
const familyLinkRequestStore = useFamilyLinkRequestStore();
const { showSnackbar } = useGlobalSnackbar();

const { add } = storeToRefs(familyLinkRequestStore);

onMounted(async () => {
  // No initial data to process
});

const handleAddFamilyLinkRequest = async () => {
  if (!familyLinkRequestFormRef.value) return;

  const valid = await familyLinkRequestFormRef.value.validate();
  if (!valid) {
    showSnackbar(t('familyLinkRequest.form.rules.allFieldsRequired'), 'error');
    return;
  }

  const formData = familyLinkRequestFormRef.value.getFormData();

  if (!formData.requestingFamilyId || !formData.targetFamilyId) {
    showSnackbar(t('familyLinkRequest.form.rules.allFieldsRequired'), 'error');
    return;
  }

  try {
    const result = await familyLinkRequestStore.createRequest(
      formData.requestingFamilyId,
      formData.targetFamilyId,
      formData.requestMessage || undefined,
    );
    if (result.ok) {
      showSnackbar(t('familyLinkRequest.requests.messages.sendSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(result.error?.message || t('familyLinkRequest.requests.messages.sendError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('familyLinkRequest.requests.messages.sendError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>