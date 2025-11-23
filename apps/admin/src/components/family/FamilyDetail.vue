<template>
  <div>
    <FamilyForm :initial-family-data="family" :read-only="props.readOnly" :title="t('family.detail.title')" />
    <v-card-actions class="justify-end pa-0">
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <v-btn v-if="canManageFamily && family" color="secondary" :loading="isGeneratingPdf" @click="generatePdf"
        data-testid="export-pdf-button">
        {{ t('family.export_pdf') }}
      </v-btn>
      <v-btn v-if="canManageFamily && family" color="info" :to="{ name: 'FamilyPdfPreview', params: { familyId: props.familyId } }"
        target="_blank" data-testid="preview-pdf-button">
        {{ t('family.preview_pdf') }}
      </v-btn>
      <v-btn color="secondary" @click="aiDrawer = true" data-testid="button-ai-input" v-if="canManageFamily">
        {{ t('common.aiInput') }}
      </v-btn>
      <v-btn color="primary" @click="editDrawer = true" data-testid="button-edit" v-if="canManageFamily">
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

    <!-- Hidden div to render the PDF template for HTML extraction -->
    <div v-if="familyPdfExportData" id="pdf-template-container" style="position: absolute; left: -9999px; width: 210mm; height: 297mm;">
        <FamilyTreePdfTemplate ref="pdfTemplateRef" :family="familyPdfExportData" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { useExportStore } from '@/stores/export.store'; // Import the new export store
import { FamilyForm } from '@/components/family';
import FamilyEditView from '@/views/family/FamilyEditView.vue';
import NLEditorView from '@/views/natural-language/NLEditorView.vue';
import type { Family } from '@/types';
import { useAuth } from '@/composables/useAuth';
import FamilyTreePdfTemplate from '@/components/family/FamilyTreePdfTemplate.vue'; // Import the new component
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { FamilyPdfExportData } from '@/types/pdf'; // Import for type hinting

const { t } = useI18n();
const router = useRouter();
const familyStore = useFamilyStore();
const exportStore = useExportStore(); // Use the new export store
const { isAdmin, isFamilyManager } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
}>();

const emit = defineEmits(['familyUpdated']);

const family = ref<Family | undefined>(undefined);
const editableFamily = ref<Family | undefined>(undefined); // Copy of family for editing
const editDrawer = ref(false); // Control visibility of the edit drawer
const aiDrawer = ref(false); // Control visibility of the AI input drawer

const familyPdfExportData = ref<FamilyPdfExportData | undefined>(undefined); // Data for the PDF template

const isGeneratingPdf = computed(() => exportStore.exportingPdf); // Use loading state from export store
const pdfTemplateRef = ref(null); // Reference to the FamilyTreePdfTemplate component

const generatePdf = async () => {
  if (!family.value) return;

  // Fetch and transform data for PDF export
  familyPdfExportData.value = await exportStore.transformFamilyToPdfExportData(props.familyId);

  if (!familyPdfExportData.value) {
    showSnackbar(t('family.export_pdf_error_data_fetch'), 'error');
    return;
  }

  exportStore.exportingPdf = true; // Set loading state manually
  try {
    // Wait for the component to render and update the DOM
    await nextTick();

    const templateContainer = document.getElementById('pdf-template-container');
    if (templateContainer) {
      // Get the full HTML content, including styles
      let htmlContent = templateContainer.innerHTML;

      // Extract styles from the <style scoped> tag within the template
      const styleTag = templateContainer.querySelector('style');
      if (styleTag) {
          htmlContent = `<style>${styleTag.innerHTML}</style>${htmlContent}`;
      }
      
      await exportStore.exportFamilyPdf(props.familyId, htmlContent); // Call action from export store
    } else {
      showSnackbar(t('family.export_pdf_error_html_gen'), 'error');
    }
  } catch (error) {
    console.error('Error generating PDF HTML:', error);
    showSnackbar(t('family.export_pdf_error'), 'error');
  } finally {
    exportStore.exportingPdf = false; // Reset loading state
  }
};

const canManageFamily = computed(() => {
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