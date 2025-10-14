<template>
  <div class="avatar-input-container mb-4">
    <!-- Current Avatar Display -->
    <div class="d-flex justify-center mb-4">
      <AvatarDisplay :src="currentAvatarSrc" :size="props.size" />
    </div>

    <!-- Upload Area -->
    <v-card class="pa-4 text-center upload-area" :class="{ 'on-drag-over': isDragOver }" flat outlined
      @click="triggerFileInput" @dragover.prevent="onDragOver" @dragleave.prevent="onDragLeave" @drop.prevent="onDrop">
      <v-icon size="48" class="mb-2">mdi-cloud-upload-outline</v-icon>
      <div class="text-subtitle-1">{{ t('avatarInput.uploadArea.clickOrDrag') }}</div>
      <div class="text-caption text-medium-emphasis">{{ t('avatarInput.uploadArea.supportedFormats') }}</div>
      <input ref="fileInput" type="file" accept="image/*" hidden @change="onFileSelected" />
    </v-card>

    <!-- Cropper Dialog -->
    <v-dialog v-model="cropperDialog" max-width="800px" persistent>
      <v-card>
        <v-card-title class="d-flex align-center">
          <v-icon icon="mdi-crop" class="mr-2"></v-icon>
          {{ t('avatarInput.cropper.title') }}
        </v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="8">
              <div class="cropper-wrapper">
                <Cropper ref="cropperRef" :src="imageForCropper" :stencil-props="{ aspectRatio: 1 / 1 }"
                  :resize-image="{ wheel: true }" image-restriction="stencil" :min-width="128" :min-height="128"
                  @change="onCropperChange" />
              </div>
            </v-col>
            <v-col cols="12" md="4">
              <div class="d-flex flex-column align-center justify-center fill-height">
                <div class="text-subtitle-1 mb-2">{{ t('avatarInput.cropper.preview') }}</div>
                <AvatarDisplay :src="croppedImagePreview" :size="128" class="mb-4" />

                <v-slider v-model="cropperZoom" :min="0.5" :max="2" :step="0.01" :label="t('avatarInput.cropper.zoom')"
                  thumb-label class="mb-4" @update:model-value="applyCropperZoom"></v-slider>

                <v-btn-toggle v-model="cropperRotate" divided class="mb-4">
                  <v-btn icon @click="rotateCropper(-90)">
                    <v-icon>mdi-rotate-left</v-icon>
                  </v-btn>
                  <v-btn icon @click="rotateCropper(90)">
                    <v-icon>mdi-rotate-right</v-icon>
                  </v-btn>
                </v-btn-toggle>
              </div>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="error" variant="text" @click="cancelCrop">{{
            t('common.cancel')
            }}</v-btn>
          <v-btn color="primary" variant="flat" @click="uploadCroppedImage" :loading="fileUploadStore.loading"
            :disabled="!isImageCropped || fileUploadStore.loading">
            {{ t('common.save') }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed, type PropType, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import AvatarDisplay from './AvatarDisplay.vue';
import { Cropper, type CropperResult } from 'vue-advanced-cropper';
import 'vue-advanced-cropper/dist/style.css';
import { useFileUploadStore } from '@/stores/fileUpload.store';
import { useNotificationStore } from '@/stores/notification.store';

const props = defineProps({
  modelValue: { type: String as PropType<string | null | undefined>, default: null },
  size: { type: Number, default: 128 },
});

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();
const fileUploadStore = useFileUploadStore();
const notificationStore = useNotificationStore();

const fileInput = ref<HTMLInputElement | null>(null);
const isDragOver = ref(false);
const cropperDialog = ref(false);
const imageForCropper = ref(''); // Base64 string for the cropper
const cropperRef = ref<InstanceType<typeof Cropper> | null>(null);
const croppedImagePreview = ref(''); // Live preview of the cropped image
const cropperZoom = ref(1);
const cropperRotate = ref(0);
const isImageCropped = ref(false); // To enable/disable save button

const currentAvatarSrc = computed(() => {
  return props.modelValue || '/images/default-avatar.png';
});

// --- File Selection & Drag/Drop ---
const triggerFileInput = () => {
  fileInput.value?.click();
};

const onDragOver = () => {
  isDragOver.value = true;
};

const onDragLeave = () => {
  isDragOver.value = false;
};

const onDrop = (event: DragEvent) => {
  isDragOver.value = false;
  if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
    processFile(event.dataTransfer.files[0]);
  }
};

const onFileSelected = (event: Event) => {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files[0]) {
    processFile(input.files[0]);
  }
  // Clear the file input value to allow selecting the same file again
  if (fileInput.value) {
    fileInput.value.value = '';
  }
};

const processFile = (file: File) => {
  if (!file.type.startsWith('image/')) {
    notificationStore.showSnackbar(t('avatarInput.uploadInput.invalidFormat'), 'error');
    return;
  }

  const reader = new FileReader();
  reader.onload = (e) => {
    imageForCropper.value = e.target?.result as string;
    cropperDialog.value = true;
    isImageCropped.value = false; // Reset cropped state
    cropperZoom.value = 1; // Reset zoom
    cropperRotate.value = 0; // Reset rotate
    nextTick(() => {
      // Ensure cropper is rendered before trying to get result for initial preview
      onCropperChange(cropperRef.value?.getResult());
    });
  };
  reader.readAsDataURL(file);
};

// --- Cropper Logic ---
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
  fileUploadStore.reset(); // Clear any loading/error state
};

const uploadCroppedImage = async () => {
  if (cropperRef.value && isImageCropped.value) {
    const { canvas } = cropperRef.value.getResult();
    if (canvas) {
      canvas.toBlob(async (blob) => {
        if (blob) {
          const croppedFile = new File([blob], `avatar_${Date.now()}.jpeg`, { type: 'image/jpeg' });
          const success = await fileUploadStore.uploadFile(croppedFile);
          if (success && fileUploadStore.uploadedUrl) {
            emit('update:modelValue', fileUploadStore.uploadedUrl);
            notificationStore.showSnackbar(t('avatarInput.uploadInput.success'), 'success');
            cropperDialog.value = false; // Close dialog on success
          } else {
            notificationStore.showSnackbar(
              fileUploadStore.error || t('avatarInput.uploadInput.error'),
              'error',
            );
          }
        }
      }, 'image/jpeg', 0.9);
    }
  }
};
</script>

<style scoped>
.avatar-input-container {
  max-width: 400px;
  margin: 0 auto;
}

.upload-area {
  cursor: pointer;
  border: 2px dashed #ccc;
  transition: border-color 0.3s ease;
}

.upload-area.on-drag-over {
  border-color: #2196f3;
  /* Vuetify blue */
  background-color: rgba(33, 150, 243, 0.1);
}

.cropper-wrapper {
  max-height: 400px;
  width: 100%;
  background: #eee;
  display: flex;
  justify-content: center;
  align-items: center;
  overflow: hidden;
}

/* Adjust cropper controls if needed */
.vue-advanced-cropper__background,
.vue-advanced-cropper__foreground {
  background: #eee;
}
</style>