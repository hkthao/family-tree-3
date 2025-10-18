<template>
  <v-card class="face-sidebar-card" elevation="2">
    <v-card-title class="text-h6">{{ t('face.sidebar.title') }}</v-card-title>
    <v-card-text class="pa-0">
      <v-list dense>
        <v-list-item v-for="face in faces" :key="face.id" :class="{
          'face-item--recognized': face.status === 'recognized',
          'face-item--unrecognized': face.status === 'unrecognized',
          'face-item--newly-labeled': face.status === 'newly-labeled',
          'face-item--selected': face.id === selectedFaceId,
        }" @click="$emit('face-selected', face.id)">
          <template v-slot:prepend>
            <v-avatar size="40" rounded="sm">
              <v-img :src="getFaceThumbnailSrc(face)" alt="Face"></v-img>
            </v-avatar>
          </template>
          <v-list-item-title>
            <span v-if="face.memberId">{{ face.memberName }}</span>
            <span v-else>{{ t('face.sidebar.unlabeled') }}</span>
          </v-list-item-title>
          <v-list-item-subtitle v-if="face.memberId">
            <div class="text-caption text-medium-emphasis">{{ face.familyName }}</div>
            <div class="text-caption text-medium-emphasis">
              <span v-if="face.birthYear">{{ face.birthYear }}</span>
              <span v-if="face.birthYear && face.deathYear"> - </span>
              <span v-if="face.deathYear">{{ face.deathYear }}</span>
            </div>
          </v-list-item-subtitle>
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
import { type PropType, computed } from 'vue';

const { t } = useI18n();

const props = defineProps({
  faces: { type: Array as () => DetectedFace[], default: () => [] },
  selectedFaceId: { type: String as PropType<string | undefined>, default: undefined },
});

const emit = defineEmits(['face-selected']);

const getFaceThumbnailSrc = (face: DetectedFace) => {
  if (face.thumbnail) {
    return `data:image/jpeg;base64,${face.thumbnail}`;
  }
  return '';
};


</script>

<style scoped>
.face-sidebar-card {
  height: 100%;
}

.face-item--recognized {
  border-left: 4px solid #4CAF50;
  /* Green */
}

.face-item--unrecognized {
  border-left: 4px solid #FF9800;
  /* Orange */
}

.face-item--newly-labeled {
  border-left: 4px solid #2196F3;
  /* Blue */
}

.face-item--selected {
  background-color: rgba(var(--v-theme-primary), 0.1);
}
</style>
