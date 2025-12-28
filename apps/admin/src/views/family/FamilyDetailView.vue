<template>
  <div v-if="isLoading">
    <v-progress-circular indeterminate color="primary"></v-progress-circular>
    {{ t('common.loading') }}
  </div>
  <div v-else-if="error">
    <v-alert type="error" :text="error?.message || t('family.detail.errorLoading')"></v-alert>
  </div>
  <div v-else-if="familyData">
    <FamilyForm :data="familyData" :read-only="props.readOnly" :title="t('family.detail.title')" :display-limit-config="true" />
    <v-card-actions class="justify-end pa-0">
      <v-btn color="gray" @click="actions.closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <v-btn color="primary" @click="actions.openEditDrawer()" data-testid="button-edit" v-if="canManageFamily">
        {{ t('common.edit') }}
      </v-btn>
      <v-btn color="info" @click="showLimitConfigDialog = true" data-testid="button-update-limits" v-if="isAdmin">
        {{ t('family.updateLimits') }}
      </v-btn>
    </v-card-actions>

    <v-dialog v-model="showLimitConfigDialog" max-width="600">
      <FamilyLimitConfigForm :family-id="props.familyId" @close="showLimitConfigDialog = false" />
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyForm } from '@/components/family';
import FamilyLimitConfigForm from '@/components/family/FamilyLimitConfigForm.vue'; // Placeholder import
import { useFamilyDetail } from '@/composables/family/logic/useFamilyDetail';
import { useAuthStore } from '@/stores/auth.store'; // Import auth store

const { t } = useI18n();
const authStore = useAuthStore();
const isAdmin = authStore.isAdmin; // Get admin status

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
}>();

const emit = defineEmits(['openEditDrawer']);

const { state: { familyData, isLoading, error, canManageFamily }, actions } = useFamilyDetail(props, emit);

const showLimitConfigDialog = ref(false); // Control for the limit config dialog
</script>