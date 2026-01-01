<template>
  <v-container fluid>
    <v-card>
      <v-card-title class="text-h5">{{ $t('menu.imageRestoration') }}</v-card-title>
      <v-card-text>
        <v-alert
          type="info"
          variant="tonal"
          class="mb-4"
          text="Ảnh cũ, mờ hoặc trầy xước có thể được AI hỗ trợ khôi phục độ rõ nét. Phục chế chỉ nhằm tăng độ rõ nét, có thể khác nguyên bản."
        ></v-alert>

        <VFileUpload
          v-model="selectedFile"
          accept="image/*"
          :title="$t('imageRestoration.uploadImage')"
          multiple
          @update:modelValue="onFileSelected"
        ></VFileUpload>

        <v-row justify="end" class="mt-4">
          <v-col cols="auto">
            <v-btn
              color="primary"
              :disabled="!selectedFile || isLoading"
              @click="restoreImage"
            >
              <v-progress-circular
                v-if="isLoading"
                indeterminate
                size="24"
                color="white"
                class="mr-2"
              ></v-progress-circular>
              {{ isLoading ? $t('imageRestoration.restoring') : $t('imageRestoration.restoreImage') }}
            </v-btn>
          </v-col>
        </v-row>

        <v-row class="mt-6" v-if="originalImageUrl || restoredImageUrl">
          <v-col cols="12" md="6">
            <v-card outlined>
              <v-img
                :src="originalImageUrl"
                aspect-ratio="1"
                class="grey lighten-2"
                cover
              >
                <template v-slot:placeholder>
                  <v-row class="fill-height ma-0" align="center" justify="center">
                    <v-progress-circular indeterminate color="grey lighten-5"></v-progress-circular>
                  </v-row>
                </template>
              </v-img>
              <v-card-text class="text-center">{{ $t('imageRestoration.originalImage') }}</v-card-text>
            </v-card>
          </v-col>
          <v-col cols="12" md="6">
            <v-card outlined>
              <v-img
                :src="restoredImageUrl"
                aspect-ratio="1"
                class="grey lighten-2"
                cover
              >
                <template v-slot:placeholder>
                  <v-row class="fill-height ma-0" align="center" justify="center">
                    <v-progress-circular indeterminate color="grey lighten-5"></v-progress-circular>
                  </v-row>
                </template>
              </v-img>
              <v-card-text class="text-center">{{ $t('imageRestoration.restoredImage') }}</v-card-text>
            </v-card>
          </v-col>
        </v-row>


      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const selectedFile = ref<File | undefined>(undefined);
const originalImageUrl = ref<string | undefined>(undefined);
const restoredImageUrl = ref<string | undefined>(undefined);
const isLoading = ref(false);

const onFileSelected = (files: File[]) => {
  if (files && files.length > 0) {
    const file = files[0];
    selectedFile.value = file;
    originalImageUrl.value = URL.createObjectURL(file);
    restoredImageUrl.value = undefined; // Clear restored image when a new file is selected
  } else {
    selectedFile.value = undefined;
    originalImageUrl.value = undefined;
    restoredImageUrl.value = undefined;
  }
};

const restoreImage = async () => {
  if (!selectedFile.value) return;

  isLoading.value = true;
  restoredImageUrl.value = undefined; // Clear previous restored image

  // Simulate API call
  await new Promise(resolve => setTimeout(resolve, 3000)); // 3 seconds delay

  // In a real scenario, you would send selectedFile.value to a backend API
  // and receive the URL of the restored image.
  // For now, let's use a placeholder image or a base64 representation.
  // Example placeholder:
  restoredImageUrl.value = 'https://via.placeholder.com/600x400/00FF00/FFFFFF?text=Restored+Image';
  // If you want to use the original image for demonstration:
  // restoredImageUrl.value = originalImageUrl.value;

  isLoading.value = false;
};
</script>

<style scoped>
/* Add any specific styles for ImageRestorationView if needed */
</style>
