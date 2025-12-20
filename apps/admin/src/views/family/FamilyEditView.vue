<template>
  <v-card :elevation="0" data-testid="family-edit-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('family.form.editTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <FamilyForm ref="familyFormRef" v-if="state.family" :data="state.family.value" :read-only="false" />
      <v-progress-circular v-else indeterminate color="primary"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="actions.closeForm" :disabled="state.isLoading.value || state.isUpdatingFamily.value">{{ t('common.cancel') }}</v-btn>
      <v-btn
        color="primary"
        data-testid="button-save"
        @click="actions.handleUpdateItem"
        :loading="state.isUpdatingFamily.value"
        :disabled="state.isLoading.value || state.isUpdatingFamily.value"
        >{{
        t('common.save')
        }}</v-btn
      >
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyForm } from '@/components/family';
import { useFamilyEdit } from '@/composables/family/logic/useFamilyEdit';
import type { FamilyAddDto, FamilyUpdateDto } from '@/types'; // Updated import

interface FamilyFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => FamilyAddDto | FamilyUpdateDto;
}

interface FamilyEditViewProps {
  familyId?: string;
}

const props = defineProps<FamilyEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyFormRef = ref<FamilyFormExposed | null>(null);

const { t } = useI18n();
const { state, actions } = useFamilyEdit(props, emit, familyFormRef);

</script>
