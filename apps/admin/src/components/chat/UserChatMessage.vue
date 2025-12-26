<template>
  <div class="d-flex align-center my-1 justify-end">
    <v-sheet class="ma-1 pa-2 text-wrap" color="primary" rounded="lg">
      <div class="message-content">
        {{ message.text }}
      </div>
      <!-- Display attachments -->
      <div v-if="message.attachments && message.attachments.length > 0" class="mt-2">
        <v-chip v-for="(attachment, index) in message.attachments" :key="index" class="ma-1"
          :prepend-icon="getAttachmentIcon(attachment.contentType)" @click="openAttachment(attachment.url)">
          {{ getFileNameFromUrl(attachment.url) }}
        </v-chip>
      </div>
      <!-- NEW: Display location -->
      <div v-if="message.location" class="mt-2">
        <v-chip class="ma-1" prepend-icon="mdi-map-marker" @click="openMapLink(message.location)">
          {{ message.location.address || `Lat: ${message.location.latitude}, Lng: ${message.location.longitude}` }}
        </v-chip>
      </div>
      <!-- END NEW -->
    </v-sheet>
    <v-avatar cover class="ml-1" size="36">
      <v-img v-if="userProfile?.value?.avatar" :src="getAvatarUrl(userProfile.value.avatar, undefined)"
        :alt="userProfile.value.name || 'User'" />
      <v-icon v-else>mdi-account-circle</v-icon>
    </v-avatar>
  </div>
</template>

<script setup lang="ts">
import type { PropType, Ref } from 'vue';
import type { AiChatMessage, ChatAttachmentDto, ChatLocation } from '@/types/chat.d'; // Import ChatAttachmentDto and ChatLocation
import { getAvatarUrl } from '@/utils/avatar.utils';
import type { UserProfile } from '@/types/user.d';

defineProps({
  message: {
    type: Object as PropType<AiChatMessage>,
    required: true,
  },
  userProfile: {
    type: Object as PropType<Ref<UserProfile | null>>,
    default: null,
  },
});

// Function to get attachment icon based on content type
const getAttachmentIcon = (contentType?: string) => {
  if (contentType?.startsWith('image/')) return 'mdi-file-image';
  if (contentType === 'application/pdf') return 'mdi-file-pdf-box';
  return 'mdi-file';
};

// Function to extract file name from URL
const getFileNameFromUrl = (url?: string) => {
  if (!url) return 'Unknown File';
  try {
    const urlObj = new URL(url);
    const pathSegments = urlObj.pathname.split('/');
    return pathSegments[pathSegments.length - 1] || 'File';
  } catch {
    return url;
  }
};

// Function to open attachment in a new tab
const openAttachment = (url?: string) => {
  if (url) {
    window.open(url, '_blank');
  }
};

// Function to open map link
const openMapLink = (location: ChatLocation) => {
  if (location.latitude && location.longitude) {
    const mapUrl = `https://www.google.com/maps/search/?api=1&query=${location.latitude},${location.longitude}`;
    window.open(mapUrl, '_blank');
  }
};
</script>

<style scoped>
.message-content {
  white-space: pre-wrap;
  word-break: break-word;
}
</style>