<template>
  <v-card :elevation="0" data-testid="family-dict-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyDict.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isAddingFamilyDict" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyDictForm ref="familyDictFormRef" @close="closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm" :disabled="isAddingFamilyDict">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddFamilyDict" data-testid="save-family-dict-button" :loading="isAddingFamilyDict" :disabled="isAddingFamilyDict">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyDictForm } from '@/components/family-dict';
import type { FamilyDict } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useAddFamilyDictMutation } from '@/composables';

const emit = defineEmits(['close', 'saved']);

const familyDictFormRef = ref<InstanceType<typeof FamilyDictForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const { mutate: addFamilyDict, isPending: isAddingFamilyDict } = useAddFamilyDictMutation();

const handleAddFamilyDict = async () => {
  if (!familyDictFormRef.value) return;
  const isValid = await familyDictFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const familyDictData = familyDictFormRef.value.getFormData();

  addFamilyDict(familyDictData as Omit<FamilyDict, 'id'>, {
    onSuccess: () => {
      showSnackbar(t('familyDict.messages.addSuccess'), 'success');
      emit('saved');
    },
    onError: (error) => {
      showSnackbar(error.message || t('familyDict.messages.saveError'), 'error');
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>
