<template>
  <v-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" max-width="800">
    <v-card>
      <v-card-title>{{ t('memoryItem.form.selectMedia') }}</v-card-title>
      <v-card-text class="v-card-text-scrollable pb-4">
        <MediaPicker :family-id="familyId" selection-mode="multiple" @selected="handleMediaSelected"
          :initial-selection="selectedMedia.map(item => item.id)" />
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn @click="closeDialog">{{ t('common.cancel') }}</v-btn>
        <v-btn color="primary" @click="confirmSelection">{{ t('common.select') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import MediaPicker from '@/components/family-media/MediaPicker.vue';
import { type MediaItem } from '@/types';

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true,
  },
  familyId: {
    type: String,
    required: true,
  },
  selectedMedia: {
    type: Array as () => MediaItem[],
    default: () => [],
  },
});

const emit = defineEmits(['update:modelValue', 'confirm']);

const { t } = useI18n();

const internalSelectedMedia = ref<MediaItem[]>([]);

watch(() => props.selectedMedia, (newVal) => {
  internalSelectedMedia.value = newVal;
}, { immediate: true });

const handleMediaSelected = (selectedItems: MediaItem[] | MediaItem | null) => {
  if (Array.isArray(selectedItems)) {
    internalSelectedMedia.value = selectedItems;
  } else if (selectedItems === null) {
    internalSelectedMedia.value = [];
  }
};

const confirmSelection = () => {
  emit('confirm', internalSelectedMedia.value);
  emit('update:modelValue', false);
};

const closeDialog = () => {
  emit('update:modelValue', false);
};
</script>

<style scoped>
.v-card-text-scrollable {
  max-height: 70vh; /* Adjust as needed */
  overflow-y: auto;
}
</style>
