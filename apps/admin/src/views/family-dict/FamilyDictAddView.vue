<template>
  <v-card :elevation="0" data-testid="family-dict-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyDict.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="add.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyDictForm ref="familyDictFormRef" @close="closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddFamilyDict" data-testid="save-family-dict-button" :loading="add.loading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyDictStore } from '@/stores/family-dict.store';
import { FamilyDictForm } from '@/components/family-dict';
import type { FamilyDict } from '@/types';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // Import useGlobalSnackbar

const emit = defineEmits(['close', 'saved']);

const familyDictFormRef = ref<InstanceType<typeof FamilyDictForm> | null>(null);

const { t } = useI18n();
const familyDictStore = useFamilyDictStore();
const { showSnackbar } = useGlobalSnackbar(); // Khởi tạo useGlobalSnackbar

const { add } = storeToRefs(familyDictStore);

onMounted(async () => {
  // No initial data to process
});

const handleAddFamilyDict = async () => {
  if (!familyDictFormRef.value) return;
  const isValid = await familyDictFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const familyDictData = familyDictFormRef.value.getFormData();

      try {
      const newFamilyDict: Omit<FamilyDict, 'id'> = {
        ...familyDictData,
      };
      await familyDictStore.addItem(newFamilyDict);
      if (!familyDictStore.error && familyDictStore.detail.item) {
        showSnackbar(t('familyDict.messages.addSuccess'), 'success');
        emit('saved');
      } else {
        showSnackbar(familyDictStore.error || t('familyDict.messages.saveError'), 'error');
      }
    } catch (error) {
      showSnackbar(t('familyDict.messages.saveError'), 'error');
    }};

const closeForm = () => {
  emit('close');
};
</script>
