<template>
  <v-card data-testid="family-add-view">
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
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{
        t('common.cancel')
      }}</v-btn>
      <v-btn
        color="primary"
        data-testid="button-save"
        @click="handleAddItem"
        >{{ t('common.save') }}</v-btn
      >
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
// import { useRouter } from 'vue-router'; // Removed as no longer used for navigation
import { useFamilyStore } from '@/stores/family.store';
import { FamilyForm } from '@/components/family';
import type { Family } from '@/types';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

const familyFormRef = ref<InstanceType<typeof FamilyForm> | null>(null);

const { t } = useI18n();
// const router = useRouter(); // Removed
const familyStore = useFamilyStore();
const { showSnackbar } = useGlobalSnackbar();

const emit = defineEmits(['close', 'saved']); // Add emit

const handleAddItem = async () => {
  if (!familyFormRef.value) return;
  const isValid = await familyFormRef.value.validate();
  if (!isValid) return;
  const itemData = familyFormRef.value.getFormData();
      try {
      await familyStore.addItem(itemData as Omit<Family, 'id'>);
      if (!familyStore.error) {
        showSnackbar(
          t('family.management.messages.addSuccess'),
          'success',
        );
        emit('close'); // Emit saved event instead of closing directly
      } else {
        showSnackbar(
          familyStore.error || t('family.management.messages.saveError'),
          'error',
        );
      }
    } catch (error) {
      showSnackbar(
        t('family.management.messages.saveError'),
        'error',
      );
    }};

const closeForm = () => {
  emit('close'); // Emit close event
};
</script>
