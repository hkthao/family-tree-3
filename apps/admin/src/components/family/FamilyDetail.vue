<template>
  <div>
    <FamilyForm :initial-family-data="family" :read-only="props.readOnly" :title="t('family.detail.title')" />
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

    <BaseCrudDrawer v-model="editDrawer" :title="t('family.form.editTitle')" icon="mdi-pencil" @close="closeEditDrawer">
      <FamilyEditView v-if="family && editDrawer" :initial-family="family" @close="closeEditDrawer"
        @saved="handleFamilySaved" />
    </BaseCrudDrawer>

    <BaseCrudDrawer v-model="aiDrawer" :title="t('aiInput.title')" icon="mdi-robot-happy-outline" @close="closeAiDrawer">
      <NLEditorView v-if="aiDrawer" :family-id="props.familyId" @close="closeAiDrawer" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { FamilyForm } from '@/components/family';
import FamilyEditView from '@/views/family/FamilyEditView.vue';
import NLEditorView from '@/views/natural-language/NLEditorView.vue';
import type { Family } from '@/types';
import { useAuth } from '@/composables/useAuth';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // New import
import { useCrudDrawer } from '@/composables/useCrudDrawer'; // New import

const { t } = useI18n();
const router = useRouter();
const familyStore = useFamilyStore();
const { isAdmin, isFamilyManager } = useAuth();

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
}>();

const emit = defineEmits(['familyUpdated', 'edit-family']); // Add emit for edit-family

const canManageFamily = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

const family = ref<Family | undefined>(undefined);

const {
  editDrawer,
  addDrawer: aiDrawer, // Rename addDrawer to aiDrawer for clarity in this context
  openEditDrawer: openEditDrawerComposable,
  openAddDrawer: openAiDrawerComposable, // Rename openAddDrawer to openAiDrawer
  closeEditDrawer,
  closeAddDrawer: closeAiDrawer, // Rename closeAddDrawer to closeAiDrawer
} = useCrudDrawer<string>();

const openEditDrawer = () => {
  if (family.value) {
    openEditDrawerComposable(family.value.id, family.value);
  }
};

const openAiDrawer = () => {
  openAiDrawerComposable();
};

const loadFamily = async () => {
  if (props.familyId) {
    await familyStore.getById(props.familyId);
    if (!familyStore.error) {
      family.value = familyStore.detail.item as Family;
    } else {
      family.value = undefined; // Clear family on error
    }
  }
};

const handleFamilySaved = async () => {
  closeEditDrawer(); // Close the edit drawer
  await loadFamily(); // Reload family data after saving
  emit('familyUpdated'); // Notify parent that family data has been updated
};

const closeView = () => {
  router.push('/family');
};

onMounted(() => {
  loadFamily();
});

watch(
  () => props.familyId,
  (newId) => {
    if (newId) {
      loadFamily();
    }
  },
);

</script>