<template>
  <div>
    <v-alert
      v-if="faces.length > 0"
      density="compact"
      type="info"
      variant="tonal"
      class="my-2"
    >
      {{ t('face.sidebar.summaryDetectedFaces', { count: faces.length }) }}
      <template v-if="unlabeledFacesCount > 0">
        <br />
        {{ t('face.sidebar.instructionClickToLabel') }}
      </template>
      <template v-else>
        <br />
        {{ t('face.sidebar.instructionNoUnlabeled') }}
      </template>
    </v-alert>

    <v-list>
      <v-list-item v-for="face in faces" :key="face.id" :class="{
        'face-item--recognized': face.status === 'recognized',
        'face-item--unrecognized': face.status === 'unrecognized',
        'face-item--newly-labeled': face.status === 'newly-labeled',
        'face-item--selected': face.id === selectedFaceId,
      }" @click="$emit('face-selected', face.id)">
        <MemberFaceDisplay :face="face" />
        <template v-slot:append>
          <v-icon v-if="face.status === 'recognized'" color="success">mdi-check-circle</v-icon>
          <v-icon v-else-if="face.status === 'unrecognized'" color="warning">mdi-alert-circle</v-icon>
          <v-icon v-else-if="face.status === 'newly-labeled'" color="info">mdi-tag</v-icon>
          <v-btn v-if="!readOnly" icon="mdi-close-circle" variant="text" size="small"
            @click.stop="removeFace(face.id)"></v-btn>
        </template>
      </v-list-item>
      <v-list-item v-if="faces.length === 0">
        <v-list-item-title>{{ t('face.sidebar.noFaces') }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { DetectedFace } from '@/types';
import { type PropType, computed } from 'vue';
import MemberFaceDisplay from '../common/MemberFaceDisplay.vue'; // NEW IMPORT

const { t } = useI18n();

const { faces, selectedFaceId, readOnly } = defineProps({
  faces: { type: Array as () => DetectedFace[], default: () => [] },
  selectedFaceId: { type: String as PropType<string | undefined>, default: undefined },
  readOnly: { type: Boolean, default: false },
});

const emit = defineEmits(['face-selected', 'remove-face']);

const removeFace = (faceId: string) => {
  emit('remove-face', faceId);
};

const unlabeledFacesCount = computed(() => faces.filter(face => !face.memberId).length);

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
