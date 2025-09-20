<template>
  <v-card>
    <v-card-text>
      <FamilyForm
        v-if="initialFamilyData"
        :initial-family-data="initialFamilyData"
        :title="t('family.form.editTitle')"
        @cancel="closeForm"
        @submit="handleUpdateFamily"
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
import { useFamilies } from '@/data/families';
import { useNotificationStore } from '@/stores/notification';
import FamilyForm from '@/components/family/FamilyForm.vue';
import type { Family } from '@/types/family';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const { getFamilyById, updateFamily } = useFamilies();
const notificationStore = useNotificationStore();

const initialFamilyData = ref<Family | null>(null);

onMounted(() => {
  const familyId = route.params.id as string;
  if (familyId) {
    const family = getFamilyById(parseInt(familyId)); // Assuming ID is number
    if (family) {
      initialFamilyData.value = { ...family };
    } else {
      notificationStore.showSnackbar(t('family.management.messages.notFound'), 'error');
      router.push('/family-management');
    }
  } else {
    router.push('/family-management');
  }
});

const handleUpdateFamily = async (familyData: Family) => {
  try {
    await updateFamily(familyData);
    notificationStore.showSnackbar(t('family.management.messages.updateSuccess'), 'success');
    closeForm();
  } catch (error) {
    notificationStore.showSnackbar(t('family.management.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  router.push('/family-management');
};
</script>