<template>
  <v-card class="face-sidebar-card" elevation="2">
    <v-card-title class="text-h6">{{ t('face.sidebar.title') }}</v-card-title>
    <v-card-text class="pa-0">
      <v-list dense>
        <v-list-item
          v-for="face in faces"
          :key="face.id"
          :class="{
            'face-item--recognized': face.status === 'recognized',
            'face-item--unrecognized': face.status === 'unrecognized',
            'face-item--newly-labeled': face.status === 'newly-labeled',
            'face-item--selected': face.id === selectedFaceId,
          }"
          @click="$emit('face-selected', face.id)"
        >
          <template v-slot:prepend>
            <v-avatar size="40" rounded="sm">
              <v-img :src="face.imageUrl" alt="Face"></v-img>
            </v-avatar>
          </template>
          <v-list-item-title>
            <span v-if="face.memberId">{{ getMemberName(face.memberId) }}</span>
            <span v-else>{{ t('face.sidebar.unlabeled') }}</span>
          </v-list-item-title>
          <template v-slot:append>
            <v-icon v-if="face.status === 'recognized'" color="success">mdi-check-circle</v-icon>
            <v-icon v-else-if="face.status === 'unrecognized'" color="warning">mdi-alert-circle</v-icon>
            <v-icon v-else-if="face.status === 'newly-labeled'" color="info">mdi-tag</v-icon>
          </template>
        </v-list-item>
        <v-list-item v-if="faces.length === 0">
          <v-list-item-title>{{ t('face.sidebar.noFaces') }}</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { DetectedFace } from '@/types';

const { t } = useI18n();

const props = defineProps({
  faces: { type: Array as () => DetectedFace[], default: () => [] },
  selectedFaceId: { type: String as PropType<string | null>, default: null },
});

const emit = defineEmits(['face-selected']);

// Mock function to get member name (replace with actual store/API call)
const getMemberName = (memberId: string) => {
  // In a real app, you'd fetch this from a member store or a lookup map
  // For now, let's assume member data is available or can be looked up
  // This is a placeholder, actual implementation would involve fetching member details
  return `Member ${memberId.substring(0, 4)}`;
};
</script>

<style scoped>
.face-sidebar-card {
  height: 100%;
}

.face-item--recognized {
  border-left: 4px solid #4CAF50; /* Green */
}

.face-item--unrecognized {
  border-left: 4px solid #FF9800; /* Orange */
}

.face-item--newly-labeled {
  border-left: 4px solid #2196F3; /* Blue */
}

.face-item--selected {
  background-color: rgba(var(--v-theme-primary), 0.1);
}
</style>
