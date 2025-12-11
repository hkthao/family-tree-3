<template>
  <div v-if="isLoading">
    <v-progress-circular indeterminate color="primary"></v-progress-circular>
    {{ t('common.loading') }}
  </div>
  <div v-else-if="error">
    <v-alert type="error" :text="error?.message || t('family.detail.errorLoading')"></v-alert>
  </div>
  <div v-else-if="familyData">
    <FamilyForm :initial-family-data="familyData" :read-only="props.readOnly" :title="t('family.detail.title')" />
    <v-card-actions class="justify-end pa-0">
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <v-btn color="secondary" @click="openAiDrawer()" data-testid="button-ai-input" v-if="canManageFamily">
        {{ t('common.aiInput') }}
      </v-btn>
      <v-btn color="primary" @click="openEditDrawer()" data-testid="button-edit" v-if="canManageFamily">
        {{ t('common.edit') }}
      </v-btn>
    </v-card-actions>
  </div>
</template>

<script setup lang="ts">
import { computed, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { FamilyForm } from '@/components/family';
import { useAuth } from '@/composables';
import { useFamilyQuery } from '@/composables/family';

const { t } = useI18n();
const router = useRouter();
const { isAdmin, isFamilyManager } = useAuth();

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
}>();

const emit = defineEmits(['openEditDrawer', 'openAiDrawer']);

const familyIdRef = toRef(props, 'familyId');
const { family: familyData, isLoading, error } = useFamilyQuery(familyIdRef);

const canManageFamily = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

const openEditDrawer = () => {
  emit('openEditDrawer', props.familyId);
};

const openAiDrawer = () => {
  emit('openAiDrawer', props.familyId);
};

const closeView = () => {
  router.push('/family');
};


</script>