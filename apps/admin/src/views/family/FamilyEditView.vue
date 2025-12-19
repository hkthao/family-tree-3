<template>
  <v-card :elevation="0" data-testid="family-edit-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('family.form.editTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <FamilyForm ref="familyFormRef" v-if="family" :data="family" :read-only="false" />
      <v-progress-circular v-else indeterminate color="primary"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm" :disabled="isLoading || isUpdatingFamily">{{ t('common.cancel') }}</v-btn>
      <v-btn
        color="primary"
        data-testid="button-save"
        @click="handleUpdateItem"
        :loading="isUpdatingFamily"
        :disabled="isLoading || isUpdatingFamily"
        >{{
        t('common.save')
        }}</v-btn
      >
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyForm } from '@/components/family';
import type { FamilyUpdateDto, FamilyAddDto } from '@/types'; // Updated import
import { useGlobalSnackbar } from '@/composables';
import { useFamilyQuery, useUpdateFamilyMutation } from '@/composables';

interface FamilyFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => FamilyAddDto | FamilyUpdateDto; // Updated return type
}

interface FamilyEditViewProps {
  familyId?: string;
}

const props = defineProps<FamilyEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyFormRef = ref<FamilyFormExposed | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { family: family, isLoading: isLoadingFamily } = useFamilyQuery(toRef(props, 'familyId') as any); // Cast for initialFamilyId, will refine if error occurs
const { mutate: updateFamily, isPending: isUpdatingFamily } = useUpdateFamilyMutation();
const isLoading = computed(() => isLoadingFamily.value);

const handleUpdateItem = async () => {
  if (!familyFormRef.value) return;
  const isValid = await familyFormRef.value.validate();
  if (!isValid) return;

  const itemData = familyFormRef.value.getFormData() as FamilyUpdateDto;
  // No need for if (!itemData.id) check as FamilyUpdateDto will always have an id



  updateFamily(itemData, {
    onSuccess: () => {
      showSnackbar(
        t('family.management.messages.updateSuccess'),
        'success',
      );
      emit('saved');
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
