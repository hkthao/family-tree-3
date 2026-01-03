<template>
  <div>
    <v-text-field
      :label="label"
      :value="displayValue"
      readonly
      @click="openMediaPicker"
      prepend-inner-icon="mdi-image-multiple"
      clearable
      @click:clear="clearSelection"
    ></v-text-field>

    <div v-if="selectedMediaLocal.length > 0 && selectionMode === 'multiple'" class="d-flex flex-wrap mt-2">
      <v-chip
        v-for="media in selectedMediaLocal"
        :key="media.id"
        class="ma-1"
        closable
        @click:close="removeMedia(media.id)"
      >
        {{ media.fileName }}
      </v-chip>
    </div>

    <div v-else-if="selectedMediaLocal.length === 1 && selectionMode === 'single'" class="mt-2">
      <v-img :src="selectedMediaLocal[0].filePath" height="100px" contain></v-img>
      <v-chip class="ma-1" closable @click:close="clearSelection">
        {{ selectedMediaLocal[0].fileName }}
      </v-chip>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
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
  allowUpload?: boolean; // New prop
}>(), {
  label: 'Select Media',
  selectionMode: 'single',
  modelValue: null,
  allowUpload: false, // Default to false
});

const emit = defineEmits(['update:modelValue']);

const mediaPickerStore = useMediaPickerDrawerStore();
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


const displayValue = computed(() => {
  if (selectedMediaLocal.value.length === 0) {
    return '';
  }
  if (props.selectionMode === 'single') {
    return selectedMediaLocal.value[0]?.fileName || '';
  }
  return `${selectedMediaLocal.value.length} media selected`;
});

const openMediaPicker = async () => {
  try {
    const initialSelectionIds = selectedMediaLocal.value.map(media => media.id);
    const result = await mediaPickerStore.openDrawer({
      familyId: props.familyId,
      selectionMode: props.selectionMode,
      initialSelection: initialSelectionIds,
      initialMediaType: props.initialMediaType,
      allowUpload: props.allowUpload,
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

const removeMedia = (idToRemove: string) => {
  selectedMediaLocal.value = selectedMediaLocal.value.filter(media => media.id !== idToRemove);
  emit('update:modelValue', props.selectionMode === 'single' ? (selectedMediaLocal.value[0] || null) : selectedMediaLocal.value);
};
</script>

<style scoped>
/* Add any specific styles for MediaInput here */
</style>
