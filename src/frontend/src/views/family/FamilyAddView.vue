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
import { useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import { FamilyForm } from '@/components/family';
import type { Family } from '@/types';

const familyFormRef = ref<InstanceType<typeof FamilyForm> | null>(null);

const { t } = useI18n();
const router = useRouter();
const familyStore = useFamilyStore();
const notificationStore = useNotificationStore();

const handleAddItem = async () => {
  if (!familyFormRef.value) return;
  const isValid = await familyFormRef.value.validate();
  if (!isValid) return;
  const itemData = familyFormRef.value.getFormData();
  try {
    await familyStore.addItem(itemData as Omit<Family, 'id'>);
    if (!familyStore.error) {
      notificationStore.showSnackbar(
        t('family.management.messages.addSuccess'),
        'success',
      );
      closeForm();
    } else {
      notificationStore.showSnackbar(
        familyStore.error || t('family.management.messages.saveError'),
        'error',
      );
    }
  } catch (error) {
    notificationStore.showSnackbar(
      t('family.management.messages.saveError'),
      'error',
    );
  }
};

const closeForm = () => {
  router.push('/family');
};
</script>
