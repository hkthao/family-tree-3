<template>
  <div>
    <!-- Tabs for media type filtering -->
    <v-tabs v-model="mediaTypeFilter" show-arrows>
      <v-tab v-for="type in visibleMediaTypes" :key="type" :value="type">
        <v-icon start>
          {{
            type === MediaType.Image ? 'mdi-image' :
            type === MediaType.Video ? 'mdi-video' :
            type === MediaType.Audio ? 'mdi-music' :
            'mdi-file-document'
          }}
        </v-icon>
        {{ t(`common.mediaType.${MediaType[type]}`) }}
      </v-tab>
    </v-tabs>

    <v-window v-model="mediaTypeFilter">
      <v-window-item :value="MediaType.Image">
        <div v-if="queryLoading" class="full-width-skeleton-container">
          <v-skeleton-loader type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
        </div>
        <div v-else-if="queryError">Error: {{ queryError.message }}</div>
        <v-alert
          v-else-if="familyMedia.length === 0"
          type="info"
          variant="tonal"
          class="ma-2"
        >{{ t('mediaPicker.noMedia') }}</v-alert>
        <div v-else class="media-grid">
          <MediaItemCard
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :media-item="mediaItem"
            :selected-media="props.selectedMedia"
            :allow-delete="props.allowDelete"
            :is-deleting="isDeleting"
            @toggle-media-selection="toggleMediaSelection"
            @delete-media="handleDeleteMedia"
          />
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Video">
        <div v-if="queryLoading" class="full-width-skeleton-container">
          <v-skeleton-loader type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
        </div>
        <div v-else-if="queryError">Error: {{ queryError.message }}</div>
        <v-alert
          v-else-if="familyMedia.length === 0"
          type="info"
          variant="tonal"
          class="ma-2"
        >{{ t('mediaPicker.noMedia') }}</v-alert>
        <div v-else class="media-grid">
          <MediaItemCard
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :media-item="mediaItem"
            :selected-media="props.selectedMedia"
            :allow-delete="props.allowDelete"
            :is-deleting="isDeleting"
            @toggle-media-selection="toggleMediaSelection"
            @delete-media="handleDeleteMedia"
          />
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Audio">
        <div v-if="queryLoading" class="full-width-skeleton-container">
          <v-skeleton-loader type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
        </div>
        <div v-else-if="queryError">Error: {{ queryError.message }}</div>
        <v-alert
          v-else-if="familyMedia.length === 0"
          type="info"
          variant="tonal"
          class="ma-2"
        >{{ t('mediaPicker.noMedia') }}</v-alert>
        <div v-else class="media-grid">
          <MediaItemCard
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :media-item="mediaItem"
            :selected-media="props.selectedMedia"
            :allow-delete="props.allowDelete"
            :is-deleting="isDeleting"
            @toggle-media-selection="toggleMediaSelection"
            @delete-media="handleDeleteMedia"
          />
        </div>
      </v-window-item>
      <v-window-item :value="MediaType.Document">
        <div v-if="queryLoading" class="full-width-skeleton-container">
          <v-skeleton-loader type="image" :key="n" v-for="n in itemsPerPage"></v-skeleton-loader>
        </div>
        <div v-else-if="queryError">Error: {{ queryError.message }}</div>
        <v-alert
          v-else-if="familyMedia.length === 0"
          type="info"
          variant="tonal"
          class="ma-2"
        >{{ t('mediaPicker.noMedia') }}</v-alert>
        <div v-else class="media-grid">
          <MediaItemCard
            v-for="mediaItem in familyMedia"
            :key="mediaItem.id"
            :media-item="mediaItem"
            :selected-media="props.selectedMedia"
            :allow-delete="props.allowDelete"
            :is-deleting="isDeleting"
            @toggle-media-selection="toggleMediaSelection"
            @delete-media="handleDeleteMedia"
          />
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
import MediaItemCard from './MediaItemCard.vue'; // Import the new component

const props = defineProps<{
  familyId: string;
  selectedMedia: string[]; // For v-model, array of media IDs
  selectionMode?: 'single' | 'multiple';
  allowDelete?: boolean; // New prop
  initialMediaType?: MediaType | null; // New prop
}>();

const emit = defineEmits<{
  (e: 'update:selectedMedia', value: FamilyMedia[]): void;
}>();

const { t } = useI18n(); // Initialize t
const queryClient = useQueryClient(); // Initialize queryClient
const { showConfirmDialog } = useConfirmDialog(); // Initialize confirm dialog
const { showSnackbar } = useGlobalSnackbar(); // Initialize snackbar

const mediaTypeFilter = ref<MediaType>(props.initialMediaType || MediaType.Image); // Default to Image
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

const visibleMediaTypes = computed(() => {
  if (props.initialMediaType !== null && props.initialMediaType !== undefined) {
    return [props.initialMediaType];
  }
  return [MediaType.Image, MediaType.Video, MediaType.Audio, MediaType.Document];
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

.full-width-skeleton-container {
  width: 100%;
}


</style>