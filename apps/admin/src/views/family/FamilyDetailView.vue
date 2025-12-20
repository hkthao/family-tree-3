<template>
  <div v-if="state.isLoading">
    <v-progress-circular indeterminate color="primary"></v-progress-circular>
    {{ t('common.loading') }}
  </div>
  <div v-else-if="state.error">
    <v-alert type="error" :text="state.error.value?.message || t('family.detail.errorLoading')"></v-alert>
  </div>
  <div v-else-if="state.familyData">
    <FamilyForm :data="state.familyData.value" :read-only="props.readOnly" :title="t('family.detail.title')" />
    <v-card-actions class="justify-end pa-0">
      <v-btn color="gray" @click="actions.closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <v-btn color="primary" @click="actions.openEditDrawer()" data-testid="button-edit" v-if="state.canManageFamily">
        {{ t('common.edit') }}
      </v-btn>
    </v-card-actions>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { FamilyForm } from '@/components/family';
import { useFamilyDetail } from '@/composables/family/logic/useFamilyDetail';

const { t } = useI18n();

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
}>();

const emit = defineEmits(['openEditDrawer']);

const { state, actions } = useFamilyDetail(props, emit);
</script>