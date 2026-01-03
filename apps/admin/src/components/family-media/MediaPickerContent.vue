<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { VContainer, VTabs, VTab, VWindow, VWindowItem, VPagination, VImg, VIcon, VSkeletonLoader, VCard, VCardText } from 'vuetify/components';
import { MediaType } from '@/types/enums'; // Use MediaType enum from enums
import { usePagination } from '@/composables/usePagination';
import { useFamilyMediaQuery } from '@/composables/queries/useFamilyMediaQuery';
import type { FamilyMedia } from '@/types'; // Import FamilyMedia type

const props = defineProps<{
  familyId: string;
  selectedMedia: string[]; // For v-model, array of media IDs
  selectionMode?: 'single' | 'multiple';
}>();

const emit = defineEmits<{
  (e: 'update:selectedMedia', value: FamilyMedia[]): void;
}>();

const mediaTypeFilter = ref<MediaType>(MediaType.Image); // Default to Image
const { currentPage, itemsPerPage, totalItems } = usePagination(1, 10);

const listOptions = computed(() => ({
  page: currentPage.value,
  itemsPerPage: itemsPerPage.value,
}));

const familyMediaFilters = computed(() => ({
  familyId: props.familyId,
  mediaType: mediaTypeFilter.value,
}));

const { familyMedia, queryLoading, queryError, totalItems: fetchedTotalItems } = useFamilyMediaQuery(
  listOptions,
  familyMediaFilters
);

watch(fetchedTotalItems, (newTotalItems) => {
  totalItems.value = newTotalItems;
});

// Watch for mediaTypeFilter changes to reset pagination
watch(mediaTypeFilter, () => {
  currentPage.value = 1;
});

// Computed property to get the full FamilyMedia objects for the selected IDs
const selectedMediaObjects = computed<FamilyMedia[]>(() => {
  return familyMedia.value.filter(media => props.selectedMedia.includes(media.id));
});

const toggleMediaSelection = (mediaId: string) => {
  let newSelectedIds: string[] = [];
  let newSelectedMediaObjects: FamilyMedia[] = [];

  if (props.selectionMode === 'single') {
    if (props.selectedMedia.includes(mediaId)) {
      newSelectedIds = []; // Deselect if already selected
    } else {
      newSelectedIds = [mediaId]; // Select new item
    }
  } else { // Multiple selection
    newSelectedIds = [...props.selectedMedia];
    const index = newSelectedIds.indexOf(mediaId);

    if (index > -1) {
      newSelectedIds.splice(index, 1); // Deselect
    } else {
      newSelectedIds.push(mediaId); // Select
    }
  }

  // Filter familyMedia to get the corresponding FamilyMedia objects
  newSelectedMediaObjects = familyMedia.value.filter(media => newSelectedIds.includes(media.id));

  emit('update:selectedMedia', newSelectedMediaObjects);
};
</script>

<template>
  <v-container fluid>
    <!-- Tabs for media type filtering -->
    <v-tabs v-model="mediaTypeFilter" show-arrows>
      <v-tab :value="MediaType.Image">Hình ảnh</v-tab>
      <v-tab :value="MediaType.Audio">Âm thanh</v-tab>
      <v-tab :value="MediaType.Document">Tài liệu</v-tab>
      <v-tab :value="MediaType.Video">Video</v-tab>
    </v-tabs>

    <v-window v-model="mediaTypeFilter">
      <v-window-item :value="MediaType.Image">
        <div class="media-grid">
          <v-skeleton-loader v-if="queryLoading" type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
          <div v-else-if="queryError">Error: {{ queryError.message }}</div>
          <div v-else-if="familyMedia.length === 0">No image media found.</div>
          <v-card
            v-else
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :class="{ 'selected-media': props.selectedMedia.includes(mediaItem.id) }"
            @click="toggleMediaSelection(mediaItem.id)"
          >
            <v-img
              v-if="mediaItem.mediaType === MediaType.Image"
              :src="mediaItem.thumbnailPath || mediaItem.filePath"
              aspect-ratio="1"
              cover
            ></v-img>
            <v-icon v-else size="64">mdi-file</v-icon> <!-- Placeholder for other types if they somehow end up here -->
            <v-card-text class="text-truncate">{{ mediaItem.fileName }}</v-card-text>
          </v-card>
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Video">
        <div class="media-grid">
          <v-skeleton-loader v-if="queryLoading" type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
          <div v-else-if="queryError">Error: {{ queryError.message }}</div>
          <div v-else-if="familyMedia.length === 0">No video media found.</div>
          <v-card
            v-else
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :class="{ 'selected-media': props.selectedMedia.includes(mediaItem.id) }"
            @click="toggleMediaSelection(mediaItem.id)"
          >
            <v-icon size="64">mdi-video</v-icon>
            <v-card-text class="text-truncate">{{ mediaItem.fileName }}</v-card-text>
          </v-card>
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Audio">
        <div class="media-grid">
          <v-skeleton-loader v-if="queryLoading" type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
          <div v-else-if="queryError">Error: {{ queryError.message }}</div>
          <div v-else-if="familyMedia.length === 0">No audio media found.</div>
          <v-card
            v-else
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :class="{ 'selected-media': props.selectedMedia.includes(mediaItem.id) }"
            @click="toggleMediaSelection(mediaItem.id)"
          >
            <v-icon size="64">mdi-audio</v-icon>
            <v-card-text class="text-truncate">{{ mediaItem.fileName }}</v-card-text>
          </v-card>
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Document">
        <div class="media-grid">
          <v-skeleton-loader v-if="queryLoading" type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
          <div v-else-if="queryError">Error: {{ queryError.message }}</div>
          <div v-else-if="familyMedia.length === 0">No document media found.</div>
          <v-card
            v-else
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :class="{ 'selected-media': props.selectedMedia.includes(mediaItem.id) }"
            @click="toggleMediaSelection(mediaItem.id)"
          >
            <v-icon size="64">mdi-file-document</v-icon>
            <v-card-text class="text-truncate">{{ mediaItem.fileName }}</v-card-text>
          </v-card>
        </div>
      </v-window-item>
    </v-window>

    <!-- Pagination controls -->
    <div class="d-flex justify-center mt-4">
      <v-pagination
        v-model="currentPage"
        :length="Math.ceil(totalItems / itemsPerPage)"
        :total-visible="7"
        :disabled="queryLoading"
      ></v-pagination>
    </div>
  </v-container>
</template>

<style scoped>
.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 16px;
  padding: 16px 0;
}

.media-grid .v-card {
  cursor: pointer;
  position: relative; /* Ensure overlay positions correctly */
  border: 2px solid transparent;
}

.media-grid .v-card.selected-media {
  border-color: rgb(var(--v-theme-primary));
}
</style>