<template>
  <v-card>
    <v-card-text>
      <FamilyForm
        v-if="initialItemData"
        :initial-family-data="initialItemData"
        :title="t('family.form.editTitle')"
        @cancel="closeForm"
        @submit="handleUpdateItem"
      />
      <v-progress-circular
        v-else
        indeterminate
        color="primary"
      ></v-progress-circular>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import FamilyForm from '@/components/family/FamilyForm.vue';
import type { Family } from '@/types/family';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const familyStore = useFamilyStore();
const notificationStore = useNotificationStore();

const initialItemData = ref<Family | null>(null);

onMounted(() => {
  const itemId = route.params.id as string;
  if (itemId) {
    const item = familyStore.items.find(f => f.id === itemId);
    if (item) {
      initialItemData.value = { ...item };
    } else {
      router.push('/family');
    }
  } else {
    router.push('/family');
  }
});

const handleUpdateItem = async (itemData: Family) => {
  try {
    await familyStore.updateItem(itemData);
    notificationStore.showSnackbar(t('family.management.messages.updateSuccess'), 'success');
    closeForm();
  } catch (error) {
    notificationStore.showSnackbar(t('family.management.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  router.push('/family');
};
</script>