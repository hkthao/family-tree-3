<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyDict.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="loading || isUpdatingFamilyDict" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyDictForm ref="familyDictFormRef" v-if="familyDict" :initial-family-dict-data="familyDict" @close="closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm" :disabled="loading || isUpdatingFamilyDict">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateFamilyDict" data-testid="save-family-dict-button" :loading="isUpdatingFamilyDict" :disabled="loading || isUpdatingFamilyDict">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyDictForm } from '@/components/family-dict';
import type { FamilyDict } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useFamilyDictQuery, useUpdateFamilyDictMutation } from '@/composables';

interface FamilyDictEditViewProps {
  familyDictId: string;
}

const props = defineProps<FamilyDictEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyDictFormRef = ref<InstanceType<typeof FamilyDictForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { familyDictId } = toRefs(props);
const { familyDict, loading } = useFamilyDictQuery(familyDictId);
const { mutate: updateFamilyDict, isPending: isUpdatingFamilyDict } = useUpdateFamilyDictMutation();

const handleUpdateFamilyDict = async () => {
  if (!familyDictFormRef.value) return;
  const isValid = await familyDictFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const familyDictData = familyDictFormRef.value.getFormData() as FamilyDict;
  if (!familyDictData.id) {
    showSnackbar(t('familyDict.messages.saveError'), 'error');
    return;
  }

  updateFamilyDict(familyDictData, {
    onSuccess: () => {
      showSnackbar(t('familyDict.messages.updateSuccess'), 'success');
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
