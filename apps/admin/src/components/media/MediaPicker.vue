<template>
  <v-container fluid>
    <v-row class="mb-4">
      <v-col cols="6">
                  <!-- eslint-disable-next-line vue/valid-v-model -->
                  <v-text-field
                    :label="t('common.search')"
                    prepend-inner-icon="mdi-magnify"
                    clearable
                    v-model="localSearchQuery"
                    hide-details
                    density="compact"
                  ></v-text-field>      </v-col>
      <v-col cols="6">
                  <!-- eslint-disable-next-line vue/valid-v-model -->
                  <v-select
                    :label="t('familyMedia.search.mediaTypeLabel')"
                    :items="[
                      { title: t('common.allMediaTypes'), value: '' },
                      { title: t('common.mediaType.Image'), value: 'Image' },
                      { title: t('common.mediaType.Video'), value: 'Video' },
                      { title: t('common.mediaType.Audio'), value: 'Audio' },
                      { title: t('common.mediaType.Document'), value: 'Document' },
                      { title: t('common.mediaType.Other'), value: 'Other' },
                    ]"
                    item-title="title"
                    item-value="value"
                    v-model="localMediaType"
                    hide-details
                    density="compact"
                  ></v-select>      </v-col>
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

    <v-row v-if="totalItems > itemsPerPage">
      <v-col cols="12" class="d-flex justify-center">
        <v-pagination
          v-model="pagination.page"
          :length="Math.ceil(totalItems / itemsPerPage)"
          :total-visible="7"
          @update:modelValue="handlePageChange"
        ></v-pagination>
      </v-col>
      <v-col cols="12" class="text-center text-caption">
        {{ t('common.itemsPerPage') }}:
        <v-menu>
          <template v-slot:activator="{ props: menuProps }">
            <v-btn
              variant="text"
              size="small"
              v-bind="menuProps"
            >
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
import { ref, watch } from 'vue'; // Removed 'computed'
import { useI18n } from 'vue-i18n';
import { useMediaPickerData } from '@/composables';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { type MediaItem } from '@/types/media';
import { MediaType } from '@/types/enums';

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

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const {
  state: { mediaList, totalItems, isLoading, pagination }, // Removed 'filters' as it's not directly used for v-model
  actions: { setPage, setItemsPerPage, setSearchQuery, setMediaType, setFamilyId },
} = useMediaPickerData({
  familyId: props.familyId,
  itemsPerPage: itemsPerPage.value,
  searchQuery: props.initialSearchQuery,
  mediaType: props.initialMediaType,
});

const internalSelectedIds = ref<string[]>([]);
const localSearchQuery = ref(props.initialSearchQuery);
const localMediaType = ref<string | MediaType | undefined>(props.initialMediaType);

// Initialize internalSelectedIds based on initialSelection prop
watch(() => props.initialSelection, (newVal) => {
  if (props.selectionMode === 'single') {
    internalSelectedIds.value = newVal ? [newVal as string] : [];
  } else {
    internalSelectedIds.value = Array.isArray(newVal) ? newVal : [];
  }
}, { immediate: true, deep: true });

// Update familyId in composable if prop changes
watch(() => props.familyId, (newFamilyId) => {
  setFamilyId(newFamilyId);
});

// Update itemsPerPage in composable if local itemsPerPage changes
watch(itemsPerPage, (newItemsPerPage) => {
  setItemsPerPage(newItemsPerPage);
});

// Watch local search query and update composable
watch(localSearchQuery, (newQuery) => {
  setSearchQuery(newQuery || '');
});

// Watch local media type and update composable
watch(localMediaType, (newType) => {
  setMediaType(newType);
});

const isSelected = (id: string) => internalSelectedIds.value.includes(id);

const toggleSelection = (id: string) => {
  if (props.selectionMode === 'single') {
    internalSelectedIds.value = isSelected(id) ? [] : [id];
  } else {
    const index = internalSelectedIds.value.indexOf(id);
    if (index > -1) {
      internalSelectedIds.value.splice(index, 1);
    }
    else {
      internalSelectedIds.value.push(id);
    }
  }
  emitSelection();
};

const emitSelection = () => {
  const selectedMediaItems = mediaList.value.filter(media => internalSelectedIds.value.includes(media.id));

  if (props.selectionMode === 'single') {
    emit('update:selection', internalSelectedIds.value[0] || '');
    emit('selected', selectedMediaItems[0] || null);
  } else {
    emit('update:selection', internalSelectedIds.value);
    emit('selected', selectedMediaItems);
  }
};

const handlePageChange = (newPage: number) => {
  setPage(newPage);
};

// No longer needed as localSearchQuery and localMediaType are watched
// const handleSearchUpdate = (query: string | null) => {
//   setSearchQuery(query || '');
// };

// const handleMediaTypeUpdate = (type: string | MediaType | null) => {
//   setMediaType(type || '');
// };

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