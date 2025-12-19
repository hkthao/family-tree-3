import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { DetectedFace, BoundingBox } from '@/types';

export function useFaceBoundingBoxViewer(props: {
  imageSrc: string;
  faces: DetectedFace[];
  selectable: boolean;
  selectedFaceId: string | null;
  loading: boolean;
}, _emit: (event: 'face-selected', face: DetectedFace) => void) {
  const { t } = useI18n();

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
    imageContainer,
    imageLoaded,
    onImageLoad,
    getBoxStyle,
  };
}