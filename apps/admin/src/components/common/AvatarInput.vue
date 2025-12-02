<template>
  <div class="avatar-input-container mb-4">
    <!-- Current Avatar Display -->
    <div class="d-flex justify-center mb-4">
      <AvatarDisplay :src="currentAvatarSrc" :size="props.size" />
    </div>
    <!-- Upload Area using VFileUpload -->
    <VFileUpload v-model="selectedFile" :label="t('avatarInput.uploadArea.clickOrDrag')" density="compact"
      :hint="t('avatarInput.uploadArea.supportedFormats')" accept="image/*" show-size counter
      prepend-icon="mdi-cloud-upload-outline" @update:modelValue="onFileSelected" />
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
          <v-btn color="primary" variant="flat" @click="uploadCroppedImage" :disabled="!isImageCropped">
            {{ t('common.save') }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>
<script setup lang="ts">
import { ref, computed, type PropType, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import AvatarDisplay from './AvatarDisplay.vue';
import { Cropper, type CropperResult } from 'vue-advanced-cropper';
import 'vue-advanced-cropper/dist/style.css';
import { VFileUpload } from 'vuetify/labs/VFileUpload';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; 
const props = defineProps({
  modelValue: { type: String as PropType<string | null | undefined>, default: null },
  size: { type: Number, default: 128 },
  initialAvatar: { type: String as PropType<string | null | undefined>, default: null }, // NEW PROP
});
const emit = defineEmits(['update:modelValue']);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar(); 
const selectedFile = ref<File[]>([]);
const cropperDialog = ref(false);
const imageForCropper = ref(''); 
const cropperRef = ref<InstanceType<typeof Cropper> | null>(null);
const croppedImagePreview = ref(''); 
const cropperZoom = ref(1);
const cropperRotate = ref(0);
const isImageCropped = ref(false); 
const currentAvatarSrc = computed(() => {
  return props.modelValue || props.initialAvatar || '/images/default-avatar.png'; // Prioritize modelValue, then initialAvatar
});
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
      onCropperChange(cropperRef.value?.getResult());
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
const uploadCroppedImage = async () => {
  if (cropperRef.value && isImageCropped.value) {
    const { canvas } = cropperRef.value.getResult();
    if (canvas) {
      const base64Image = canvas.toDataURL('image/jpeg', 0.9);
      emit('update:modelValue', base64Image);
      showSnackbar(t('avatarInput.uploadInput.success'), 'success');
      cropperDialog.value = false; 
    } else {
      showSnackbar(t('avatarInput.uploadInput.error'), 'error'); 
    }
  }
};
</script>
<style scoped>
.avatar-input-container {
  max-width: 400px;
  margin: 0 auto;
}
/* Removed custom upload-area styles as VFileUpload handles styling */
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