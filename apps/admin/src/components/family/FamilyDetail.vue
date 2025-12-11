<template>
  <div>
    <FamilyForm :initial-family-data="props.familyData" :read-only="props.readOnly" :title="t('family.detail.title')" />
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
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { FamilyForm } from '@/components/family';
import type { Family } from '@/types';
import { useAuth } from '@/composables';

const { t } = useI18n();
const router = useRouter();
const { isAdmin, isFamilyManager } = useAuth();

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
  familyData?: Family; // New prop to receive family data
}>();

const emit = defineEmits(['openEditDrawer', 'openAiDrawer']);

const canManageFamily = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

// The local 'family' ref is no longer needed as data comes via prop
// const family = ref<Family | undefined>(undefined);

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