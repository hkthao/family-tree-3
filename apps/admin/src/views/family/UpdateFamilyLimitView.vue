<template>
  <v-card elevation="0">
    <v-card-title class="text-center">{{ t('family.updateLimits') }}</v-card-title>
    <v-card-text>
      <div v-if="state.isLoading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="state.error">
        <v-alert type="error" :text="state.error?.message || t('family.form.errorLoadLimits')"></v-alert>
      </div>
      <template v-else-if="state.familyLimitData">
        <FamilyLimitConfigForm
          ref="familyLimitConfigFormRef"
          :family-id="props.familyId"
          :family-limit-data="state.familyLimitData"
          :is-saving="state.isUpdating"
        />
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="gray" @click="actions.closeForm">{{ t('common.cancel') }}</v-btn>
          <v-btn color="primary" @click="actions.handleActionSave" :loading="state.isUpdating">{{ t('common.save') }}</v-btn>
        </v-card-actions>
      </template>
      <div v-else>
        <v-alert type="info">{{ t('common.noData') }}</v-alert>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import FamilyLimitConfigForm from '@/components/family/FamilyLimitConfigForm.vue';
import { useUpdateFamilyLimit } from '@/composables/family/logic/useUpdateFamilyLimit';

const { t } = useI18n();

const props = defineProps<{
  familyId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const familyLimitConfigFormRef = ref<InstanceType<typeof FamilyLimitConfigForm> | null>(null);

const { state, actions } = useUpdateFamilyLimit(props, emit, familyLimitConfigFormRef);
</script>

<style scoped></style>
