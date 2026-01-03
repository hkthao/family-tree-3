<template>
  <div>
    <!-- Tabs for media type filtering -->
    <v-tabs v-model="mediaTypeFilter" show-arrows>
      <v-tab :value="MediaType.Image">{{ t('common.mediaType.Image') }}</v-tab>
      <v-tab :value="MediaType.Audio">{{ t('common.mediaType.Audio') }}</v-tab>
      <v-tab :value="MediaType.Document">{{ t('common.mediaType.Document') }}</v-tab>
      <v-tab :value="MediaType.Video">{{ t('common.mediaType.Video') }}</v-tab>
    </v-tabs>

    <v-window v-model="mediaTypeFilter">
      <v-window-item :value="MediaType.Image">
        <div v-if="queryLoading" class="full-width-skeleton-container">
          <v-skeleton-loader type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
        </div>
        <div v-else-if="queryError">Error: {{ queryError.message }}</div>
        <div v-else-if="familyMedia.length === 0">{{ t('mediaPicker.noMedia') }}</div>
        <div v-else class="media-grid">
          <v-card
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :class="{ 'selected-media': props.selectedMedia.includes(mediaItem.id) }"
            @click="toggleMediaSelection(mediaItem.id)"
            class="media-item"
            variant="outlined"
            color="primary"
          >
            <v-img
              v-if="mediaItem.mediaType === MediaType.Image"
              :src="mediaItem.thumbnailPath || mediaItem.filePath"
              aspect-ratio="1"
              cover
            ></v-img>
            <v-icon v-else size="64">mdi-file</v-icon> <!-- Placeholder for other types if they somehow end up here -->
            <v-icon
              v-if="props.selectedMedia.includes(mediaItem.id)"
              class="selected-check-icon"
              color="primary"
              size="24"
            >
              mdi-check-circle
            </v-icon>
            <v-btn
              v-if="props.allowDelete"
              class="delete-media-button"
              icon
              size="small"
              color="error"
              variant="text"
              :disabled="isDeleting"
              @click.stop="handleDeleteMedia(mediaItem)"
            >
              <v-icon>mdi-delete</v-icon>
            </v-btn>
            <v-card-text class="text-truncate">{{ mediaItem.fileName }}</v-card-text>
          </v-card>
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Video">
        <div v-if="queryLoading" class="full-width-skeleton-container">
          <v-skeleton-loader type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
        </div>
        <div v-else-if="queryError">Error: {{ queryError.message }}</div>
        <div v-else-if="familyMedia.length === 0">{{ t('mediaPicker.noMedia') }}</div>
        <div v-else class="media-grid">
          <v-card
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            variant="outlined"
            color="primary"
            :class="{ 'selected-media': props.selectedMedia.includes(mediaItem.id) }"
            @click="toggleMediaSelection(mediaItem.id)"
            class="media-item"
          >
            <v-icon size="64">mdi-video</v-icon>
            <v-icon
              v-if="props.selectedMedia.includes(mediaItem.id)"
              class="selected-check-icon"
              color="primary"
              size="24"
            >
              mdi-check-circle
            </v-icon>
            <v-btn
              v-if="props.allowDelete"
              class="delete-media-button"
              icon
              size="small"
              color="error"
              variant="text"
              :disabled="isDeleting"
              @click.stop="handleDeleteMedia(mediaItem)"
            >
              <v-icon>mdi-delete</v-icon>
            </v-btn>
            <v-card-text class="text-truncate">{{ mediaItem.fileName }}</v-card-text>
          </v-card>
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Audio">
        <div v-if="queryLoading" class="full-width-skeleton-container">
          <v-skeleton-loader type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
        </div>
        <div v-else-if="queryError">Error: {{ queryError.message }}</div>
        <div v-else-if="familyMedia.length === 0">{{ t('mediaPicker.noMedia') }}</div>
        <div v-else class="media-grid">
          <v-card
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            variant="outlined"
            color="primary"
            :class="{ 'selected-media': props.selectedMedia.includes(mediaItem.id) }"
            @click="toggleMediaSelection(mediaItem.id)"
            class="media-item"
          >
            <v-icon size="64">mdi-audio</v-icon>
            <v-icon
              v-if="props.selectedMedia.includes(mediaItem.id)"
              class="selected-check-icon"
              color="primary"
              size="24"
            >
              mdi-check-circle
            </v-icon>
            <v-btn
              v-if="props.allowDelete"
              class="delete-media-button"
              icon
              size="small"
              color="error"
              variant="flat"
              :disabled="isDeleting"
              @click.stop="handleDeleteMedia(mediaItem)"
            >
              <v-icon>mdi-delete</v-icon>
            </v-btn>
            <v-card-text class="text-truncate">{{ mediaItem.fileName }}</v-card-text>
          </v-card>
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Document">
        <div v-if="queryLoading" class="full-width-skeleton-container">
          <v-skeleton-loader type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
        </div>
        <div v-else-if="queryError">Error: {{ queryError.message }}</div>
        <div v-else-if="familyMedia.length === 0">{{ t('mediaPicker.noMedia') }}</div>
        <div v-else class="media-grid">
          <v-card
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            variant="outlined"
            color="primary"
            :class="{ 'selected-media': props.selectedMedia.includes(mediaItem.id) }"
            @click="toggleMediaSelection(mediaItem.id)"
            class="media-item"
          >
            <v-icon size="64">mdi-file-document</v-icon>
            <v-icon
              v-if="props.selectedMedia.includes(mediaItem.id)"
              class="selected-check-icon"
              color="primary"
              size="24"
            >
              mdi-check-circle
            </v-icon>
            <v-btn
              v-if="props.allowDelete"
              class="delete-media-button"
              icon
              size="small"
              color="error"
              variant="text"
              :disabled="isDeleting"
              @click.stop="handleDeleteMedia(mediaItem)"
            >
              <v-icon>mdi-delete</v-icon>
            </v-btn>
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n'; // Import useI18n
import { useQueryClient } from '@tanstack/vue-query'; // Import useQueryClient
import { MediaType } from '@/types/enums'; // Use MediaType enum from enums
import { usePagination } from '@/composables/usePagination';
import { useFamilyMediaQuery } from '@/composables/queries/useFamilyMediaQuery';
import { useFamilyMediaDeleteMutation } from '@/composables/family-media/useFamilyMediaDeleteMutation'; // Import delete mutation
import { useConfirmDialog, useGlobalSnackbar } from '@/composables'; // Import for confirmation and snackbar
import type { FamilyMedia } from '@/types'; // Import FamilyMedia type

const props = defineProps<{
  familyId: string;
  selectedMedia: string[]; // For v-model, array of media IDs
  selectionMode?: 'single' | 'multiple';
  allowDelete?: boolean; // New prop
}>();

const emit = defineEmits<{
  (e: 'update:selectedMedia', value: FamilyMedia[]): void;
}>();

const { t } = useI18n(); // Initialize t
const queryClient = useQueryClient(); // Initialize queryClient
const { showConfirmDialog } = useConfirmDialog(); // Initialize confirm dialog
const { showSnackbar } = useGlobalSnackbar(); // Initialize snackbar

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

// --- Delete functionality ---
const { mutate: deleteMedia, isPending: isDeleting } = useFamilyMediaDeleteMutation();

const handleDeleteMedia = async (mediaItem: FamilyMedia) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyMedia.list.confirmDelete', { fileName: mediaItem.fileName }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    deleteMedia(mediaItem.id, {
      onSuccess: () => {
        showSnackbar(t('familyMedia.messages.deleteSuccess'), 'success');
        // Remove the deleted item from current selection if it was selected
        if (props.selectedMedia.includes(mediaItem.id)) {
          const newSelection = selectedMediaObjects.value.filter(item => item.id !== mediaItem.id);
          emit('update:selectedMedia', newSelection);
        }
        queryClient.invalidateQueries({ queryKey: ['familyMedia'] }); // Refresh media list
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyMedia.messages.deleteError'), 'error');
      },
    });
  }
};
</script>

<style scoped>
.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 16px;
  padding: 16px 0;
}

.media-grid .media-item {
  cursor: pointer;
  position: relative; /* Ensure check icon positions correctly */
}

.media-grid .media-item.selected-media {
  border-color: rgb(var(--v-theme-primary));
}

.selected-check-icon {
  position: absolute;
  top: 4px;
  right: 4px;
  background-color: white; /* Optional: for better visibility */
  border-radius: 50%; /* Makes the background circular */
  padding: 2px; /* Add some padding around the icon */
  z-index: 1; /* Ensure it's above other content */
}

.delete-media-button {
  position: absolute;
  top: 4px;
  left: 4px;
  z-index: 2; /* Ensure it's above other content */
}

.full-width-skeleton-container {
  width: 100%;
}
</style>