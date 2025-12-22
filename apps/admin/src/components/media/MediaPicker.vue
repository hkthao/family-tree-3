<template>
  <v-container fluid>
    <v-row class="mb-4">
      <v-col cols="6">
        <v-text-field :label="t('common.search')" prepend-inner-icon="mdi-magnify" clearable v-model="localSearchQuery"
          hide-details density="compact"></v-text-field> </v-col>
      <v-col cols="6">
        <v-select :label="t('familyMedia.search.mediaTypeLabel')" :items="[
          { title: t('common.allMediaTypes'), value: '' },
          { title: t('common.mediaType.Image'), value: 'Image' },
          { title: t('common.mediaType.Video'), value: 'Video' },
          { title: t('common.mediaType.Audio'), value: 'Audio' },
          { title: t('common.mediaType.Document'), value: 'Document' },
          { title: t('common.mediaType.Other'), value: 'Other' },
        ]" item-title="title" item-value="value" v-model="localMediaType" hide-details
          density="compact"></v-select> </v-col>
    </v-row>

    <v-row v-if="isLoading">
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
        <v-card class="media-item-card d-flex align-center justify-center" :class="{ 'selected': isSelected(media.id) }"
          flat outlined>
          <v-img :src="media.url" aspect-ratio="1" cover class="media-image"></v-img>
          <v-overlay :model-value="isSelected(media.id)" contained scrim="#000000" class="align-center justify-center">
            <v-icon color="white" size="48">mdi-check-circle</v-icon>
          </v-overlay>
        </v-card>
      </v-col>
    </v-row>

    <v-row v-if="totalItems > itemsPerPage">
      <v-col cols="12" class="d-flex justify-center">
        <v-pagination v-model="pagination.page" :length="Math.ceil(totalItems / itemsPerPage)" :total-visible="7"
          @update:modelValue="handlePageChange"></v-pagination>
      </v-col>
      <v-col cols="12" class="text-center text-caption">
        {{ t('common.itemsPerPage') }}:
        <v-menu>
          <template v-slot:activator="{ props: menuProps }">
            <v-btn variant="text" size="small" v-bind="menuProps">
              {{ itemsPerPage }}
            </v-btn>
          </template>
          <v-list>
            <v-list-item v-for="size in [10, 25, 50, 100]" :key="size" @click="itemsPerPage = size">
              <v-list-item-title>{{ size }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useMediaPickerData } from '@/composables/family-media';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { type MediaItem } from '@/types';
import { useMediaPickerLogic } from '@/composables/family-media';

type SelectionMode = 'single' | 'multiple';

const props = withDefaults(defineProps<{
  familyId: string;
  selectionMode?: SelectionMode;
  initialSelection?: string[] | string;
  initialSearchQuery?: string;
  initialMediaType?: string;
}>(), {
  selectionMode: 'single',
  initialSelection: () => [],
  initialSearchQuery: '',
  initialMediaType: '',
});

const emit = defineEmits<{
  (e: 'update:selection', value: string[] | string): void;
  (e: 'selected', value: MediaItem[] | MediaItem | null): void;
}>();

const { t } = useI18n();

const {
  state: { mediaList, totalItems, isLoading, pagination },
  actions: { setPage, setItemsPerPage, setSearchQuery, setMediaType, setFamilyId },
} = useMediaPickerData({
  familyId: props.familyId,
  itemsPerPage: DEFAULT_ITEMS_PER_PAGE, // Use constant here, will be passed to logic
  searchQuery: props.initialSearchQuery,
  mediaType: props.initialMediaType,
});

const {
  state: { itemsPerPage, localSearchQuery, localMediaType },
  actions: { toggleSelection, handlePageChange, isSelected },
} = useMediaPickerLogic({
  familyId: props.familyId,
  selectionMode: props.selectionMode,
  initialSelection: props.initialSelection,
  initialSearchQuery: props.initialSearchQuery,
  initialMediaType: props.initialMediaType,
  mediaList: mediaList, // Pass mediaList Ref directly
  setPage,
  setItemsPerPage,
  setSearchQuery,
  setMediaType,
  setFamilyId,
  emit,
  DEFAULT_ITEMS_PER_PAGE,
});


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