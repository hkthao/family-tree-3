<template>
  <v-container fluid>
    <v-row v-if="loading">
      <v-col cols="12" class="text-center">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
      </v-col>
    </v-row>
    <v-row v-else-if="mediaList.length === 0">
      <v-col cols="12" class="text-center text-medium-emphasis">
        {{ t('mediaPicker.noMedia') }}
      </v-col>
    </v-row>
    <v-row v-else>
      <v-col v-for="media in mediaList" :key="media.id" cols="4" @click="toggleSelection(media.id)">
        <v-card
          class="media-item-card d-flex align-center justify-center"
          :class="{ 'selected': isSelected(media.id) }"
          flat
          outlined
        >
          <v-img
            :src="media.url"
            aspect-ratio="1"
            cover
            class="media-image"
          ></v-img>
          <v-overlay
            v-model="isSelected(media.id)"
            contained
            scrim="#000000"
            class="align-center justify-center"
          >
            <v-icon color="white" size="48">mdi-check-circle</v-icon>
          </v-overlay>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';

import { type MediaItem } from '@/types/media';

type SelectionMode = 'single' | 'multiple';

const props = withDefaults(defineProps<{
  mediaList: MediaItem[];
  selectionMode?: SelectionMode;
  initialSelection?: string[] | string;
  loading?: boolean;
}>(), {
  selectionMode: 'single',
  initialSelection: () => [],
  loading: false,
});

const emit = defineEmits<{
  (e: 'update:selection', value: string[] | string): void;
  (e: 'selected', value: MediaItem[] | MediaItem): void;
}>();

const { t } = useI18n();

const internalSelectedIds = ref<string[]>([]);

// Initialize internalSelectedIds based on initialSelection prop
watch(() => props.initialSelection, (newVal) => {
  if (props.selectionMode === 'single') {
    internalSelectedIds.value = newVal ? [newVal as string] : [];
  } else {
    internalSelectedIds.value = Array.isArray(newVal) ? newVal : [];
  }
}, { immediate: true });

const isSelected = (id: string) => internalSelectedIds.value.includes(id);

const toggleSelection = (id: string) => {
  if (props.selectionMode === 'single') {
    internalSelectedIds.value = isSelected(id) ? [] : [id];
  } else {
    const index = internalSelectedIds.value.indexOf(id);
    if (index > -1) {
      internalSelectedIds.value.splice(index, 1);
    } else {
      internalSelectedIds.value.push(id);
    }
  }
  emitSelection();
};

const emitSelection = () => {
  const selectedMediaItems = props.mediaList.filter(media => internalSelectedIds.value.includes(media.id));

  if (props.selectionMode === 'single') {
    emit('update:selection', internalSelectedIds.value[0] || '');
    emit('selected', selectedMediaItems[0] || null);
  } else {
    emit('update:selection', internalSelectedIds.value);
    emit('selected', selectedMediaItems);
  }
};

</script>

<style scoped>
.media-item-card {
  cursor: pointer;
  position: relative;
  border: 2px solid transparent;
  transition: border-color 0.2s ease-in-out;
}

.media-item-card.selected {
  border-color: rgb(var(--v-theme-primary));
}

.media-image {
  max-width: 100%;
  height: auto;
}
</style>