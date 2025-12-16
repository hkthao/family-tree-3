<template>
  <v-card :elevation="0" data-testid="family-location-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLocation.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isAddingFamilyLocation" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLocationForm ref="familyLocationFormRef" :family-id="props.familyId" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm" :disabled="isAddingFamilyLocation">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddFamilyLocation" data-testid="save-family-location-button" :loading="isAddingFamilyLocation" :disabled="isAddingFamilyLocation">{{
        t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>
<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyLocationForm } from '@/components/family-location'; // Assuming index.ts re-exports it
import type { FamilyLocation } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useAddFamilyLocationMutation } from '@/composables/family-location'; // Import new composable

interface FamilyLocationAddViewProps {
  familyId: string;
}
const props = defineProps<FamilyLocationAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyLocationFormRef = ref<InstanceType<typeof FamilyLocationForm> | null>(null);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { mutate: addFamilyLocation, isPending: isAddingFamilyLocation } = useAddFamilyLocationMutation();

const handleAddFamilyLocation = async () => {
  if (!familyLocationFormRef.value) return;
  const isValid = await familyLocationFormRef.value.validate();
  if (!isValid) {
    return;
  }
  const familyLocationData = familyLocationFormRef.value.getFormData();
  familyLocationData.familyId = props.familyId;

  addFamilyLocation(familyLocationData as Omit<FamilyLocation, 'id'>, {
    onSuccess: () => {
      showSnackbar(t('familyLocation.messages.addSuccess'), 'success');
      emit('saved');
    },
    onError: (error) => {
      showSnackbar(error.message || t('familyLocation.messages.saveError'), 'error');
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>
