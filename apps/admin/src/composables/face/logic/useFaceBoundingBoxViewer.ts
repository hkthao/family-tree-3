import { ref, watch, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, BoundingBox } from '@/types';

export function useFaceBoundingBoxViewer(props: {
  imageSrc: string;
  faces: DetectedFace[];
  selectable: boolean;
  selectedFaceId: string | null;
  loading: boolean;
  imageContainer: Ref<HTMLElement | null>; // Change to Ref<HTMLElement | null>
}, _emit: (event: 'face-selected', face: DetectedFace) => void) {
  const { t } = useI18n();

  const imageLoaded = ref(false);
  const naturalWidth = ref(0);
  const naturalHeight = ref(0);

  // Use a local ref to hold the actual HTMLElement value
  const containerElement = ref<HTMLElement | null>(null);

  watch(() => props.imageContainer.value, (newVal) => {
    containerElement.value = newVal;
  }, { immediate: true }); // Watch immediately to catch initial value

  const onImageLoad = (event: Event) => {
    const img = event.target as HTMLImageElement;
    naturalWidth.value = img.naturalWidth;
    naturalHeight.value = img.naturalHeight;
    imageLoaded.value = true;
  };

  const getBoxStyle = (box: BoundingBox | null | undefined) => {
    if (!box || !imageLoaded.value || !containerElement.value) return {};

    const containerWidth = containerElement.value.offsetWidth;
    const containerHeight = containerElement.value.offsetHeight;

    // Calculate scaling factors
    const scaleX = containerWidth / naturalWidth.value;
    const scaleY = containerHeight / naturalHeight.value;

    const style = {
      left: `${box.x * scaleX}px`,
      top: `${box.y * scaleY}px`,
      width: `${box.width * scaleX}px`,
      height: `${box.height * scaleY}px`,
    };
    return style;
  };

  // Watch for imageSrc changes to reset imageLoaded state
  watch(() => props.imageSrc, () => {
    imageLoaded.value = false;
    naturalWidth.value = 0;
    naturalHeight.value = 0;
  });

  return {
    t,
    imageLoaded,
    onImageLoad,
    getBoxStyle,
  };
}