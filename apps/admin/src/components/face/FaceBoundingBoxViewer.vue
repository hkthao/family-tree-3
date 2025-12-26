<template>
  <v-card class="face-viewer-card" :loading="loading">
    <div class="image-container" ref="imageContainer">
      <img :src="imageSrc" class="responsive-image" @load="onImageLoad" />
      <div v-if="imageLoaded" class="bounding-boxes-overlay">
        <div v-for="face in faces" :key="face.id" class="bounding-box" :style="getBoxStyle(face.boundingBox)" :class="{
          'bounding-box--recognized': face.status === 'recognized',
          'bounding-box--unrecognized': face.status === 'unrecognized',
          'bounding-box--newly-labeled': face.status === 'newly-labeled',
          'bounding-box--selected': selectable && face.id === selectedFaceId,
          'bounding-box--selectable': selectable && face.status !== 'recognized', // Only selectable if not recognized
          'bounding-box--disabled': face.status === 'recognized', // NEW: Disabled class for recognized faces
        }" @click="selectable && face.status !== 'recognized' && $emit('face-selected', face)"> <!-- NEW: Prevent click if recognized -->
          <div v-if="face.memberId" class="bounding-box__name">
            {{ face.memberName }}
          </div>
          <v-tooltip activator="parent" location="bottom">
            <span v-if="face.memberId">{{ t('face.boundingBox.labeledAs', { name: face.memberName })
            }}</span>
            <span v-else>{{ t('face.boundingBox.unlabeled') }}</span>
          </v-tooltip>
        </div>
      </div>
    </div>
  </v-card>
</template>

<script setup lang="ts">
import { type PropType } from 'vue';
import type { DetectedFace } from '@/types';
import { useFaceBoundingBoxViewer } from '@/composables';

const props = defineProps({
  imageSrc: { type: String, required: true },
  faces: { type: Array as () => DetectedFace[], default: () => [] },
  selectable: { type: Boolean, default: false },
  selectedFaceId: { type: String as PropType<string | null>, default: null },
  loading: { type: Boolean, default: false },
});

const emit = defineEmits(['face-selected']);

const {
  t,
  imageLoaded,
  onImageLoad,
  getBoxStyle,
} = useFaceBoundingBoxViewer(props, emit);
</script>

<style scoped>
.face-viewer-card {
  position: relative;
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  overflow: hidden;
}

.image-container {
  position: relative;
  max-width: 100%;
  max-height: 100%;
  display: inline-block;
  /* To make overlay position correctly */
}

.responsive-image {
  max-width: 100%;
  height: auto;
  display: block;
}

.bounding-boxes-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
}

.bounding-box {
  position: absolute;
  border: 2px solid;
  box-sizing: border-box;
  cursor: default;
  transition: border-color 0.2s ease-in-out;
}

.bounding-box--selectable {
  cursor: pointer;
}

.bounding-box--recognized {
  border-color: #4CAF50;
  /* Green */
}

.bounding-box--unrecognized {
  border-color: #FF9800;
  /* Orange */
}

.bounding-box--newly-labeled {
  border-color: #2196F3;
  /* Blue */
}

.bounding-box--selected {
  border-width: 4px;
  filter: brightness(1.2);
}

.bounding-box__name {
  position: absolute;
  top: -40px;
  left: -15px;
  background-color: rgba(0, 0, 0, 0.7);
  color: white;
  padding: 2px 5px;
  font-size: 0.70rem;
  overflow: hidden;
  border-radius: 3px;
  z-index: 10;
  min-width: 90px;
  height: 35px;
  text-align: center;;
}

.bounding-box--disabled {
  opacity: 0.6;
  cursor: not-allowed !important;
  border-style: dashed;
}
</style>
