<template>
  <v-card flat>
    <v-card-title class="text-h6">{{ t('aiInput.families') }}</v-card-title>
    <v-card-text>
      <div v-for="(family, index) in families" :key="index" class="mb-4">
        <FamilyForm :initial-family-data="family" ref="familyFormRefs" />
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family } from '@/types';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import FamilyForm from '@/components/family/FamilyForm.vue';

const props = defineProps({
  families: {
    type: Array as () => Family[],
    required: true,
  },
});

const emit = defineEmits(['saved']);

const { t } = useI18n();
const familyStore = useFamilyStore();
const notificationStore = useNotificationStore();

const familyFormRefs = ref<InstanceType<typeof FamilyForm>[] | null>(null);

const saveFamilies = async () => {
  let allFormsValid = true;
  const familiesToSave: Family[] = [];

  if (familyFormRefs.value) {
    for (const formRef of familyFormRefs.value) {
      const isValid = await formRef.validate();
      if (!isValid) {
        allFormsValid = false;
        break;
      }
      familiesToSave.push(formRef.getFormData() as Family);
    }
  }

  if (!allFormsValid) {
    notificationStore.showSnackbar(
      t('aiInput.validationError'),
      'error',
    );
    return;
  }

  try {
    for (const family of familiesToSave) {
      await familyStore.addItem(family);
    }
    notificationStore.showSnackbar(
      t('aiInput.saveSuccess'),
      'success',
    );
    emit('saved');
  } catch (error) {
    console.error('Error saving families:', error);
    notificationStore.showSnackbar(
      t('aiInput.saveError'),
      'error',
    );
  }
};

defineExpose({
  saveFamilies,
});
</script>
