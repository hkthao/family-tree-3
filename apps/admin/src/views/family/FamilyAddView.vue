<template>
  <v-card :elevation="0" data-testid="family-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('family.form.addTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <FamilyForm ref="familyFormRef" @cancel="closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm" :disabled="isAddingFamily">{{
        t('common.cancel')
        }}</v-btn>
      <v-btn color="primary" data-testid="button-save" @click="handleAddItem" :loading="isAddingFamily"
        :disabled="isAddingFamily">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyForm } from '@/components/family';
import type { Family } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useAddFamilyMutation } from '@/composables/family';

const familyFormRef = ref<InstanceType<typeof FamilyForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const { mutate: addFamily, isPending: isAddingFamily } = useAddFamilyMutation();

const emit = defineEmits(['close', 'saved']);

const handleAddItem = async () => {
  if (!familyFormRef.value) return;
  const isValid = await familyFormRef.value.validate();
  if (!isValid) return;
  const itemData = familyFormRef.value.getFormData();
  addFamily(itemData as Omit<Family, 'id'>, {
    onSuccess: () => {
      showSnackbar(
        t('family.management.messages.addSuccess'),
        'success',
      );
      emit('close');
    },
    onError: (error) => {
      showSnackbar(
        error.message || t('family.management.messages.saveError'),
        'error',
      );
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>
