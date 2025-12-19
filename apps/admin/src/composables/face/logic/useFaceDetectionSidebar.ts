import { useI18n } from 'vue-i18n';
import type { DetectedFace } from '@/types';
import { computed } from 'vue';

export function useFaceDetectionSidebar(props: {
  faces: DetectedFace[];
  selectedFaceId?: string | undefined;
  readOnly: boolean;
}, emit: (event: 'face-selected' | 'remove-face', ...args: any[]) => void) {
  const { t } = useI18n();

  const removeFace = (faceId: string) => {
    emit('remove-face', faceId);
  };

  const unlabeledFacesCount = computed(() => props.faces.filter(face => !face.memberId).length);

  return {
    t,
    removeFace,
    unlabeledFacesCount,
  };
}