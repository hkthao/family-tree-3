<template>
  <v-card
    variant="outlined"
    color="primary"
    :class="{ 'selected-media': isSelected }"
    @click="emit('toggleMediaSelection', mediaItem.id)"
    class="media-item"
  >
    <div v-if="mediaItem.mediaType !== MediaType.Image" class="media-icon-container">
      <v-icon size="64">
        {{
          mediaItem.mediaType === MediaType.Video ? 'mdi-video' :
          mediaItem.mediaType === MediaType.Audio ? 'mdi-music' :
          'mdi-file-document'
        }}
      </v-icon>
    </div>
    <v-img
      v-else-if="mediaItem.mediaType === MediaType.Image"
      :src="mediaItem.thumbnailPath || mediaItem.filePath"
      aspect-ratio="1"
      cover
    ></v-img>

    <v-icon
      v-if="isSelected"
      class="selected-check-icon"
      color="primary"
      size="24"
    >
      mdi-check-circle
    </v-icon>
    <v-btn
      v-if="allowDelete"
      class="delete-media-button"
      icon
      size="small"
      color="error"
      variant="text"
      :disabled="isDeleting"
      @click.stop="emit('deleteMedia', mediaItem)"
    >
      <v-icon>mdi-delete</v-icon>
    </v-btn>
    <v-card-text class="text-truncate">{{ mediaItem.fileName }}</v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { MediaType } from '@/types/enums';
import type { FamilyMedia } from '@/types';

const props = defineProps<{
  mediaItem: FamilyMedia;
  selectedMedia: string[]; // IDs of selected media
  allowDelete: boolean;
  isDeleting: boolean;
}>();

const emit = defineEmits<{
  (e: 'toggleMediaSelection', mediaId: string): void;
  (e: 'deleteMedia', mediaItem: FamilyMedia): void;
}>();

const isSelected = computed(() => props.selectedMedia.includes(props.mediaItem.id));
</script>

<style scoped>
/* Styles for media-item, selected-check-icon, delete-media-button, media-icon-container will go here */
.media-item {
  cursor: pointer;
  position: relative; /* Ensure check icon positions correctly */
}

.media-item.selected-media {
  border-color: rgb(var(--v-theme-primary));
}

.selected-check-icon {
  position: absolute;
  top: 4px;
  left: 4px;
  background-color: white; /* Optional: for better visibility */
  border-radius: 50%; /* Makes the background circular */
  padding: 2px; /* Add some padding around the icon */
  z-index: 1; /* Ensure it's above other content */
}

.delete-media-button {
  position: absolute;
  top: 4px;
  right: 4px;
  z-index: 2; /* Ensure it's above other content */
}

.media-icon-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 150px; /* Example height, adjust as needed */
  background-color: #f0f0f0; /* Placeholder background */
}
</style>
