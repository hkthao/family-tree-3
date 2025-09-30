<template>
  <div>
    <div class="d-flex justify-center mb-4">
      <AvatarDisplay :src="previewSrc" :size="props.size" />
    </div>

    <v-tabs v-model="tab" grow>
      <v-tab value="url">{{ t('avatarInput.tab.url') }}</v-tab>
      <v-tab value="upload">{{ t('avatarInput.tab.upload') }}</v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <v-window-item value="url">
        <div class="d-flex flex-row align-center my-4">
          <v-text-field
            v-model="imageUrl"
            :label="t('avatarInput.urlInput.label')"
            clearable
            hide-details
          ></v-text-field>
          <v-btn class="ml-4" @click="confirmUrl" :disabled="!imageUrl">{{
            t('avatarInput.urlInput.confirm')
          }}</v-btn>
        </div>
      </v-window-item>
      <v-window-item value="upload">
        <v-file-input
          v-model="selectedFile"
          :label="t('avatarInput.uploadInput.label')"
          accept="image/*"
          prepend-icon="mdi-camera"
          show-size
          class="mt-4"
          @change="onFileSelected"
        ></v-file-input>

        <v-dialog v-model="cropperDialog" max-width="600px">
          <v-card>
            <v-card-title>{{ t('avatarInput.cropper.title') }}</v-card-title>
            <v-card-text>
              <div class="cropper-wrapper">
                <Cropper
                  ref="cropperRef"
                  :src="imageForCropper"
                  :stencil-props="{ aspectRatio: 1 / 1 }"
                  :resize-image="false"
                  image-restriction="stencil"
                />
              </div>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="blue-darken-1" @click="cropperDialog = false">{{
                t('common.cancel')
              }}</v-btn>
              <v-btn color="blue-darken-1" @click="cropImage">{{
                t('common.save')
              }}</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </v-window-item>
    </v-window>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import AvatarDisplay from './AvatarDisplay.vue';
import { Cropper } from 'vue-advanced-cropper';
import 'vue-advanced-cropper/dist/style.css';

const props = defineProps({
  modelValue: { type: String, default: null },
  size: { type: Number, default: 128 },
});

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

const tab = ref('url');
const imageUrl = ref(props.modelValue || '');
const selectedFile = ref<File[]>([]);
const cropperDialog = ref(false);
const imageForCropper = ref('');
const cropperRef = ref<InstanceType<typeof Cropper> | null>(null);

const previewSrc = computed(() => props.modelValue || imageUrl.value);

watch(
  () => props.modelValue,
  (newValue) => {
    if (newValue && tab.value === 'url') {
      imageUrl.value = newValue;
    }
  },
);

const confirmUrl = () => {
  emit('update:modelValue', imageUrl.value);
};

const onFileSelected = (event: Event) => {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files[0]) {
    const file = input.files[0];
    const reader = new FileReader();
    reader.onload = (e) => {
      imageForCropper.value = e.target?.result as string;
      cropperDialog.value = true;
    };
    reader.readAsDataURL(file);
  }
};

const cropImage = () => {
  if (cropperRef.value) {
    const { canvas } = cropperRef.value.getResult();
    if (canvas) {
      // Resize and export to base64
      const resizedCanvas = document.createElement('canvas');
      const ctx = resizedCanvas.getContext('2d');
      const maxWidth = 256; // Max width for resized image
      const maxHeight = 256; // Max height for resized image

      let width = canvas.width;
      let height = canvas.height;

      if (width > height) {
        if (width > maxWidth) {
          height *= maxWidth / width;
          width = maxWidth;
        }
      } else {
        if (height > maxHeight) {
          width *= maxHeight / height;
          height = maxHeight;
        }
      }

      resizedCanvas.width = width;
      resizedCanvas.height = height;
      ctx?.drawImage(canvas, 0, 0, width, height);

      const base64Image = resizedCanvas.toDataURL('image/jpeg', 0.9); // Export as JPEG with 90% quality
      emit('update:modelValue', base64Image);
      cropperDialog.value = false;
      selectedFile.value = []; // Clear selected file input
    }
  }
};
</script>

<style scoped>
.cropper-wrapper {
  max-height: 400px;
  width: 100%;
  background: #eee;
}
</style>
