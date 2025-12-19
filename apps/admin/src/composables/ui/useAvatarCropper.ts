import { ref, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import type { CropperResult } from 'vue-advanced-cropper';
import { useGlobalSnackbar } from '@/composables';

export function useAvatarCropper() {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const selectedFile = ref<File[]>([]);
  const cropperDialog = ref(false);
  const imageForCropper = ref('');
  const cropperRef = ref<any | null>(null); // Type any for Cropper instance
  const croppedImagePreview = ref('');
  const cropperZoom = ref(1);
  const cropperRotate = ref(0);
  const isImageCropped = ref(false);

  const onFileSelected = (files: File[]) => {
    if (files && files.length > 0) {
      processFile(files[0]);
    }
  };

  const processFile = (file: File) => {
    if (!file.type.startsWith('image/')) {
      showSnackbar(t('avatarInput.uploadInput.invalidFormat'), 'error');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      imageForCropper.value = e.target?.result as string;
      cropperDialog.value = true;
      isImageCropped.value = false;
      cropperZoom.value = 1;
      cropperRotate.value = 0;
      nextTick(() => {
        // Ensure cropperRef is available before calling getResult
        if (cropperRef.value) {
          onCropperChange(cropperRef.value.getResult());
        }
      });
    };
    reader.readAsDataURL(file);
  };

  const onCropperChange = (result: CropperResult | undefined) => {
    if (result && result.canvas) {
      croppedImagePreview.value = result.canvas.toDataURL('image/jpeg', 0.9);
      isImageCropped.value = true;
    } else {
      isImageCropped.value = false;
    }
  };

  const applyCropperZoom = (value: number) => {
    if (cropperRef.value) {
      cropperRef.value.zoom(value);
    }
  };

  const rotateCropper = (angle: number) => {
    if (cropperRef.value) {
      cropperRef.value.rotate(angle);
    }
  };

  const cancelCrop = () => {
    cropperDialog.value = false;
    imageForCropper.value = '';
    croppedImagePreview.value = '';
    isImageCropped.value = false;
    selectedFile.value = [];
  };

  // This function will be called from the parent component
  const getCroppedImageBase64 = () => {
    if (cropperRef.value && isImageCropped.value) {
      const { canvas } = cropperRef.value.getResult();
      if (canvas) {
        return canvas.toDataURL('image/jpeg', 0.9);
      }
    }
    return null;
  };

  return {
    selectedFile,
    cropperDialog,
    imageForCropper,
    cropperRef,
    croppedImagePreview,
    cropperZoom,
    cropperRotate,
    isImageCropped,
    onFileSelected,
    onCropperChange,
    applyCropperZoom,
    rotateCropper,
    cancelCrop,
    getCroppedImageBase64,
  };
}
