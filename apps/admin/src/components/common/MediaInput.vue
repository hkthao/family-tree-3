<template>
  <v-sheet class="d-flex flex-column border rounded pa-2">
    <div v-if="selectedMediaLocal.length > 0 && selectionMode === 'multiple'" class="d-flex flex-wrap mt-2">
      <v-chip v-for="media in selectedMediaLocal" :key="media.id" class="ma-1" :closable="true"
        @click:close="removeMedia(media.id)">
        {{ media.fileName }}
      </v-chip>
    </div>

    <div v-else-if="selectedMediaLocal.length === 1 && selectionMode === 'single'" class="mt-2">
      <v-img :src="selectedMediaLocal[0].filePath" height="100px" contain></v-img>
      <v-chip class="ma-1" :closable="true" @click:close="clearSelection">
        {{ selectedMediaLocal[0].fileName }}
      </v-chip>
    </div>
    <div class="mt-4 text-center">
      <v-btn :prepend-icon="'mdi-image-multiple'" @click="openMediaPicker" class="flex-grow-1 justify-start">
        <span class="text-truncate">{{ t('common.selectFile') }}</span>
      </v-btn>
    </div>
  </v-sheet>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n'; // Import useI18n
import { useMediaPickerDrawerStore } from '@/stores/mediaPickerDrawer.store';
import type { FamilyMedia } from '@/types';
import { MediaType } from '@/types/enums';

type SelectionMode = 'single' | 'multiple';

const props = withDefaults(defineProps<{
  label?: string;
  familyId: string;
  selectionMode?: SelectionMode;
  modelValue: FamilyMedia[] | FamilyMedia | null; // v-model
  initialMediaType?: MediaType;

}>(), {
  label: 'Select Media',
  selectionMode: 'single',
  modelValue: null,

});

const emit = defineEmits(['update:modelValue']);

const mediaPickerStore = useMediaPickerDrawerStore();
const { t } = useI18n(); // Initialize t
const selectedMediaLocal = ref<FamilyMedia[]>([]);


onMounted(() => {
  // Initialize local state from modelValue
  if (props.modelValue) {
    selectedMediaLocal.value = Array.isArray(props.modelValue) ? props.modelValue : [props.modelValue];
  }
});

watch(() => props.modelValue, (newVal) => {
  if (newVal) {
    selectedMediaLocal.value = Array.isArray(newVal) ? newVal : [newVal];
  } else {
    selectedMediaLocal.value = [];
  }
}, { deep: true });

const openMediaPicker = async () => {
  try {
    const initialSelectionIds = selectedMediaLocal.value.map(media => media.id);
          const result = await mediaPickerStore.openDrawer({
            familyId: props.familyId,
            selectionMode: props.selectionMode,
            initialSelection: initialSelectionIds,
            initialMediaType: props.initialMediaType,
          });
    if (props.selectionMode === 'single') {
      if (result && !Array.isArray(result)) {
        selectedMediaLocal.value = [result];
      } else {
        selectedMediaLocal.value = [];
      }
    } else { // multiple
      if (result && Array.isArray(result)) {
        selectedMediaLocal.value = result;
      } else {
        selectedMediaLocal.value = [];
      }
    }
    emit('update:modelValue', props.selectionMode === 'single' ? (selectedMediaLocal.value[0] || null) : selectedMediaLocal.value);

  } catch (error) {
    console.error('Media selection cancelled or failed:', error);
  }
};

const clearSelection = () => {
  selectedMediaLocal.value = [];
  emit('update:modelValue', props.selectionMode === 'single' ? null : []);
};

const removeMedia = async (idToRemove: string) => {
  selectedMediaLocal.value = selectedMediaLocal.value.filter(media => media.id !== idToRemove);
  emit('update:modelValue', props.selectionMode === 'single' ? (selectedMediaLocal.value[0] || null) : selectedMediaLocal.value);
};
</script>

<style scoped>
/* Add any specific styles for MediaInput here */
</style>
