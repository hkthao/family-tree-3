<template>
  <v-card :elevation="0" data-testid="family-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('family.form.addTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <FamilyForm ref="familyFormRef" @cancel="actions.closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="actions.closeForm" :disabled="state.isAddingFamily.value">{{
        t('common.cancel')
        }}</v-btn>
      <v-btn color="primary" data-testid="button-save" @click="actions.handleAddItem" :loading="state.isAddingFamily.value"
        :disabled="state.isAddingFamily.value">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { FamilyForm } from '@/components/family';
import { useFamilyAdd } from '@/composables/family/logic/useFamilyAdd';
import { useI18n } from 'vue-i18n';

const emit = defineEmits(['close', 'saved']);
const { t } = useI18n();
const { state, actions } = useFamilyAdd(emit);
const { familyFormRef } = state;
</script>
