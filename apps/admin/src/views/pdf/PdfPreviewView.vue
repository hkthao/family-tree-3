<template>
  <div class="pdf-preview-container" :class="{ 'loading-overlay': loading }">
    <v-progress-circular v-if="loading" indeterminate color="primary" size="64"></v-progress-circular>
    <div v-else-if="familyPdfData" class="pdf-content-wrapper">
      <FamilyTreePdfTemplate :family="familyPdfData" />
    </div>
    <v-alert v-else type="error" dense prominent>
      Không thể tải dữ liệu gia phả để xem trước PDF.
    </v-alert>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useExportStore } from '@/stores/export.store';
import FamilyTreePdfTemplate from '@/components/family/FamilyTreePdfTemplate.vue';
import type { FamilyPdfExportData } from '@/types/pdf';

const route = useRoute();
const exportStore = useExportStore();

const familyPdfData = ref<FamilyPdfExportData | undefined>(undefined);
const loading = ref(true);

const props = defineProps<{
  familyId: string;
}>();

const loadFamilyData = async (id: string) => {
  loading.value = true;
  familyPdfData.value = await exportStore.transformFamilyToPdfExportData(id);
  loading.value = false;
};

onMounted(() => {
  if (props.familyId) {
    loadFamilyData(props.familyId);
  }
});

watch(() => props.familyId, (newId) => {
  if (newId) {
    loadFamilyData(newId);
  }
});
</script>

<style scoped>
.pdf-preview-container {
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #f0f2f5; /* Light gray background */
  padding: 20px;
}

.loading-overlay {
  background-color: rgba(255, 255, 255, 0.8);
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.pdf-content-wrapper {
  background-color: #ffffff;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
  width: 210mm; /* A4 width */
  min-height: 297mm; /* A4 height */
  overflow: auto; /* In case content overflows */
}

/* Deeply styled template might need to adjust page breaks */
.pdf-content-wrapper::v-deep .family-tree-pdf-template {
  margin: 0; /* Override default template margin */
  background-color: #ffffff; /* Ensure white background for preview */
  box-shadow: none;
}
</style>