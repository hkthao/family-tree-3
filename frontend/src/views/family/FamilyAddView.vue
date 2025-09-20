<template>
  <v-card>
    <v-card-text>
      <FamilyForm
        :title="t('family.form.addTitle')"
        @cancel="closeForm"
        @submit="handleAddFamily"
      />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilies } from '@/data/families';
import { useNotificationStore } from '@/stores/notification';
import FamilyForm from '@/components/family/FamilyForm.vue';
import type { Family } from '@/types/family';

const { t } = useI18n();
const router = useRouter();
const { addFamily } = useFamilies();
const notificationStore = useNotificationStore();

const handleAddFamily = async (familyData: Omit<Family, 'id'>) => {
  try {
    await addFamily(familyData);
    notificationStore.showSnackbar(t('family.management.messages.addSuccess'), 'success');
    closeForm();
  } catch (error) {
    notificationStore.showSnackbar(t('family.management.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  router.push('/family-management');
};
</script>