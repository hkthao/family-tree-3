<template>
  <v-card class="face-viewer-card" :loading="loading">
    <div class="image-container" ref="imageContainer">
      <img :src="imageSrc" class="responsive-image" @load="onImageLoad" />
      <div v-if="imageLoaded" class="bounding-boxes-overlay">
        <div
          v-for="face in faces"
          :key="face.id"
          class="bounding-box"
          :style="getBoxStyle(face.boundingBox)"
          :class="{
            'bounding-box--recognized': face.status === 'recognized',
            'bounding-box--unrecognized': face.status === 'unrecognized',
            'bounding-box--newly-labeled': face.status === 'newly-labeled',
            'bounding-box--selected': selectable && face.id === selectedFaceId,
            'bounding-box--selectable': selectable,
          }"
          @click="selectable && $emit('face-selected', face.id)"
        >
          <v-tooltip activator="parent" location="bottom">
            <span v-if="face.memberId">{{ t('face.boundingBox.labeledAs', { name: getMemberName(face.memberId) }) }}</span>
            <span v-else>{{ t('face.boundingBox.unlabeled') }}</span>
          </v-tooltip>
        </div>
      </div>
    </div>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, BoundingBox } from '@/types';
import { useFaceStore } from '@/stores/face.store';

const { t } = useI18n();
const faceStore = useFaceStore();

const props = defineProps({
  imageSrc: { type: String, required: true },
  faces: { type: Array as () => DetectedFace[], default: () => [] },
  selectable: { type: Boolean, default: false },
  selectedFaceId: { type: String as PropType<string | null>, default: null },
  loading: { type: Boolean, default: false },
});

const emit = defineEmits(['face-selected']);

const imageContainer = ref<HTMLElement | null>(null);
const imageLoaded = ref(false);
const naturalWidth = ref(0);
const naturalHeight = ref(0);

const onImageLoad = (event: Event) => {
  const img = event.target as HTMLImageElement;
  naturalWidth.value = img.naturalWidth;
  naturalHeight.value = img.naturalHeight;
  imageLoaded.value = true;
};

const getBoxStyle = (box: BoundingBox | null | undefined) => {
  if (!box || !imageLoaded.value || !imageContainer.value) return {};

  const containerWidth = imageContainer.value.offsetWidth;
  const containerHeight = imageContainer.value.offsetHeight;

  // Calculate scaling factors
  const scaleX = containerWidth / naturalWidth.value;
  const scaleY = containerHeight / naturalHeight.value;

  const style = {
    left: `${box.x * scaleX}px`,
    top: `${box.y * scaleY}px`,
    width: `${box.width * scaleX}px`,
    height: `${box.height * scaleY}px`,
  };

  console.log('BoundingBox:', box);
  console.log('Scale factors:', { scaleX, scaleY });
  console.log('Calculated style:', style);

  return style;
};

// Mock function to get member name (replace with actual store/API call)
const getMemberName = (memberId: string | null | undefined) => {
  if (memberId) {
    // In a real app, you'd fetch this from a member store or a lookup map
    // For now, return a placeholder
    return `Member ${memberId.substring(0, 4)}`;
  }
  return '';
};

// Watch for imageSrc changes to reset imageLoaded state
watch(() => props.imageSrc, () => {
  imageLoaded.value = false;
  naturalWidth.value = 0;
  naturalHeight.value = 0;
});
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
  display: inline-block; /* To make overlay position correctly */
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
  border-color: #4CAF50; /* Green */
}

.bounding-box--unrecognized {
  border-color: #FF9800; /* Orange */
}

.bounding-box--newly-labeled {
  border-color: #2196F3; /* Blue */
}

.bounding-box--selected {
  border-width: 4px;
  filter: brightness(1.2);
}
</style>
