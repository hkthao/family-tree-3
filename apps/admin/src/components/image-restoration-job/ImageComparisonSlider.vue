<template>
  <div class="image-comparison-slider-container">
    <v-img
      :src="beforeImageSrc"
      cover
      class="image-comparison-before"
    >
      <template v-slot:placeholder>
        <v-row class="fill-height ma-0" align="center" justify="center">
          <v-progress-circular indeterminate color="grey lighten-5"></v-progress-circular>
        </v-row>
      </template>
    </v-img>

    <v-img
      :src="afterImageSrc"
      cover
      class="image-comparison-after"
      :style="{ clipPath: `inset(0 ${100 - sliderValue}% 0 0)` }"
    >
      <template v-slot:placeholder>
        <v-row class="fill-height ma-0" align="center" justify="center">
          <v-progress-circular indeterminate color="grey lighten-5"></v-progress-circular>
        </v-row>
      </template>
    </v-img>


    <div
      class="comparison-divider"
      :style="{ left: `${sliderValue}%` }"
      @mousedown="startDrag"
    >
      <div class="divider-handle"></div>
    </div>
    <div class="image-comparison-labels">
      <span class="label-before">{{ t('imageRestorationJob.comparison.original') }}</span>
      <span class="label-after">{{ t('imageRestorationJob.comparison.restored') }}</span>
    </div>

    <v-snackbar
      v-model="showHint"
      :timeout="5000"
      color="info"
      location="bottom right"
    >
      {{ t('imageRestorationJob.comparison.hint') }}
      <template v-slot:actions>
        <v-btn
          variant="text"
          @click="showHint = false"
        >
          {{ t('common.close') }}
        </v-btn>
      </template>
    </v-snackbar>

    <div v-if="restorationLevel || restorationTime" class="restoration-info-overlay">
      <span v-if="restorationLevel">{{ t('imageRestorationJob.comparison.level', { level: restorationLevel }) }}</span>
      <span v-if="restorationLevel && restorationTime"> | </span>
      <span v-if="restorationTime">{{ t('imageRestorationJob.comparison.time', { time: restorationTime }) }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useI18n } from 'vue-i18n';

const HINT_SHOWN_KEY = 'imageComparisonSliderHintShown';

defineProps({
  beforeImageSrc: {
    type: String,
    required: true,
  },
  afterImageSrc: {
    type: String,
    required: true,
  },
  restorationLevel: {
    type: String,
    default: undefined,
  },
  restorationTime: {
    type: [String, Number],
    default: undefined,
  },
});

const { t } = useI18n();
const sliderValue = ref(50); // Default to 50%
const showHint = ref(false);
const isDragging = ref(false);
let containerRect: DOMRect | null = null; // Store container dimensions

const startDrag = (event: MouseEvent) => {
  isDragging.value = true;
  // Get the bounding rectangle of the container for accurate position calculation
  const container = document.querySelector('.image-comparison-slider-container');
  if (container) {
    containerRect = container.getBoundingClientRect();
  }
  // Prevent default to avoid image dragging or other native browser behaviors
  event.preventDefault();
};

const onDrag = (event: MouseEvent) => {
  if (!isDragging.value || !containerRect) return;

  // Calculate new slider value based on mouse X position relative to container
  const newValue = ((event.clientX - containerRect.left) / containerRect.width) * 100;

  // Clamp the value between 0 and 100
  sliderValue.value = Math.max(0, Math.min(100, newValue));
};

const stopDrag = () => {
  isDragging.value = false;
  containerRect = null; // Clear container rect
};

onMounted(() => {
  if (!localStorage.getItem(HINT_SHOWN_KEY)) {
    showHint.value = true;
    localStorage.setItem(HINT_SHOWN_KEY, 'true');
  }

  // Add global event listeners for dragging
  window.addEventListener('mousemove', onDrag);
  window.addEventListener('mouseup', stopDrag);
});

onUnmounted(() => {
  // Clean up global event listeners
  window.removeEventListener('mousemove', onDrag);
  window.removeEventListener('mouseup', stopDrag);
});

</script>

<style scoped>
.image-comparison-slider-container {
  position: relative;
  width: 100%;
  height: 90vh; /* Set height to 90% of viewport height */
  overflow: hidden;
  border-radius: 8px; /* Added for a softer look */
  background-color: #f0f0f0; /* Fallback background */
}

.image-comparison-before,
.image-comparison-after {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: contain; /* Use 'contain' to ensure the entire image is visible within 90vh */
  pointer-events: none; /* Allow interaction with slider underneath */
}

.comparison-divider {
  position: absolute;
  top: 0;
  bottom: 0;
  width: 2px;
  background-color: rgba(255, 255, 255, 0.5); /* Semi-transparent white line */
  cursor: ew-resize;
  z-index: 10;
  transform: translateX(-50%); /* Center the line on the 'left' position */
}

.divider-handle {
  width: 30px;
  height: 30px;
  background-color: white;
  border-radius: 50%;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  box-shadow: 0 0 8px rgba(0, 0, 0, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
}

.divider-handle::before,
.divider-handle::after {
  content: '';
  position: absolute;
  width: 2px;
  height: 10px;
  background-color: #616161;
}

.divider-handle::before {
  transform: translateX(-4px);
}

.divider-handle::after {
  transform: translateX(4px);
}




.image-comparison-labels {
  position: absolute;
  top: 10px;
  left: 10px;
  right: 10px; /* Extend to the right */
  display: flex;
  flex-direction: row; /* Stack labels horizontally */
  justify-content: space-between; /* Distribute labels */
  z-index: 5;
  color: white;
  font-weight: bold;
  text-shadow: 1px 1px 2px rgba(0,0,0,0.7);
}

.label-before, .label-after {
  background-color: rgba(0, 0, 0, 0.5);
  padding: 2px 5px;
  border-radius: 3px;
  font-size: 0.8em;
}

.restoration-info-overlay {
  position: absolute;
  bottom: 10px;
  left: 10px; /* Move to bottom-left */
  background-color: rgba(0, 0, 0, 0.6);
  color: white;
  padding: 5px 10px;
  border-radius: 5px;
  font-size: 0.8em;
  z-index: 15;
}
</style>