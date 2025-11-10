<template>
  <div>
    <FamilyForm :initial-family-data="family" :read-only="props.readOnly" :title="t('family.detail.title')" />
    <v-card-actions class="justify-end pa-0">
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <v-btn color="secondary" @click="aiDrawer = true" data-testid="button-ai-input" v-if="canEditFamily">
        {{ t('common.aiInput') }}
      </v-btn>
      <v-btn color="primary" @click="editDrawer = true" data-testid="button-edit" v-if="canEditFamily">
        {{ t('common.edit') }}
      </v-btn>
    </v-card-actions>

    <v-navigation-drawer v-model="editDrawer" location="right" temporary width="650">
      <FamilyEditView v-if="editableFamily && editDrawer" :initial-family="editableFamily" @close="editDrawer = false"
        @saved="handleFamilySaved" />
    </v-navigation-drawer>

    <v-navigation-drawer v-model="aiDrawer" location="right" temporary width="650">
      <NLEditorView v-if="aiDrawer" :family-id="props.familyId" @close="aiDrawer = false" />
    </v-navigation-drawer>
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

const { t } = useI18n();
const router = useRouter();
const familyStore = useFamilyStore();
const { isAdmin, isFamilyManager } = useAuth();

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
}>();

const emit = defineEmits(['familyUpdated']);

const family = ref<Family | undefined>(undefined);
const editableFamily = ref<Family | undefined>(undefined); // Copy of family for editing
const editDrawer = ref(false); // Control visibility of the edit drawer
const aiDrawer = ref(false); // Control visibility of the AI input drawer

const canEditFamily = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

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
  editDrawer.value = false;
  await loadFamily(); // Reload family data after saving
  emit('familyUpdated'); // Notify parent that family data has been updated
};

const closeView = () => {
  router.push('/family');
};

onMounted(() => {
  loadFamily();
});

watch(editDrawer, (newVal) => {
  if (newVal && family.value) {
    editableFamily.value = JSON.parse(JSON.stringify(family.value));
  }
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