<template>
  <v-card  elevation="0">
    <v-card-title class="text-center">{{ t('family.updateLimits') }}</v-card-title>
    <v-card-text>
      <div v-if="isLoading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert type="error" :text="error?.message || t('family.form.errorLoadLimits')"></v-alert>
      </div>
      <template v-else-if="familyLimitData">
        <FamilyLimitConfigForm
          ref="familyLimitConfigFormRef"
          :family-id="props.familyId"
          :family-limit-data="familyLimitData"
          :is-saving="isUpdating"
          @save="handleFormSave"
        />
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="gray" @click="emit('close')">{{ t('common.cancel') }}</v-btn>
          <v-btn color="primary" @click="handleActionSave" :loading="isUpdating">{{ t('common.save') }}</v-btn>
        </v-card-actions>
      </template>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import FamilyLimitConfigForm from '@/components/family/FamilyLimitConfigForm.vue';
import { useFamilyLimitConfiguration } from '@/composables/family/logic/useFamilyLimitConfiguration';
import type { FamilyLimitConfiguration } from '@/types/family.d';

const { t } = useI18n();

const props = defineProps<{
  familyId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const familyLimitConfigFormRef = ref<InstanceType<typeof FamilyLimitConfigForm> | null>(null);

const { isLoading, error, familyLimitData, isUpdating, updateFamilyLimits, updateMutation } = useFamilyLimitConfiguration(props.familyId);

const handleFormSave = async (payload: FamilyLimitConfiguration) => {
  updateFamilyLimits({
    maxMembers: payload.maxMembers,
    maxStorageMb: payload.maxStorageMb,
    aiChatMonthlyLimit: payload.aiChatMonthlyLimit,
  });
};

const handleActionSave = async () => {
  if (familyLimitConfigFormRef.value) {
    // This will trigger the emit('save') from the child if validation passes
    await familyLimitConfigFormRef.value.handleSave();
  }
};

// Watch for the mutation status to emit events to the parent
watch(() => updateMutation.isSuccess, (isSuccess) => {
  if (isSuccess) {
    emit('saved');
    emit('close');
  }
});

watch(() => updateMutation.isError, (isError) => {
  if (isError) {
    emit('close'); // Close on error as well, or handle error display more explicitly
  }
});
</script>

<style scoped></style>
