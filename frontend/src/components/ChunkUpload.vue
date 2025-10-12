<template>
  <v-card class="pa-4">
    <v-card-title class="text-h5">Tải lên tài liệu</v-card-title>
    <v-card-text>
      <v-file-input
        v-model="selectedFile"
        label="Chọn tệp PDF hoặc TXT"
        accept=".pdf,.txt"
        prepend-icon="mdi-paperclip"
        show-size
        counter
        :rules="fileRules"
        @change="onFileChange"
      ></v-file-input>

      <v-text-field
        v-model="fileId"
        label="ID Tệp (Duy nhất)"
        :rules="[v => !!v || 'ID Tệp là bắt buộc']"
        required
      ></v-text-field>

      <v-text-field
        v-model="familyId"
        label="ID Gia đình"
        :rules="[v => !!v || 'ID Gia đình là bắt buộc']"
        required
      ></v-text-field>

      <v-text-field
        v-model="category"
        label="Danh mục (ví dụ: Tiểu sử, Lịch sử)"
        :rules="[v => !!v || 'Danh mục là bắt buộc']"
        required
      ></v-text-field>

      <v-text-field
        v-model="createdBy"
        label="Người tạo (ID người dùng)"
        :rules="[v => !!v || 'Người tạo là bắt buộc']"
        required
      ></v-text-field>

      <v-progress-linear
        v-if="loading"
        indeterminate
        color="primary"
        class="mb-3"
      ></v-progress-linear>

      <v-alert v-if="error" type="error" dense dismissible class="mb-3">{{ error }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="primary" :disabled="!isFormValid || loading" @click="upload">
        Tải lên
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useChunkStore } from '@/stores/chunk.store';

const chunkStore = useChunkStore();

const selectedFile = ref<File | null>(null);
const fileId = ref('');
const familyId = ref('');
const category = ref('');
const createdBy = ref('');

const loading = computed(() => chunkStore.loading);
const error = computed(() => chunkStore.error);

const fileRules = [
  (value: File) => !!value || 'Tệp là bắt buộc',
  (value: File) => {
    const allowedTypes = ['application/pdf', 'text/plain'];
    return allowedTypes.includes(value?.type) || 'Chỉ chấp nhận tệp PDF hoặc TXT.';
  },
];

const isFormValid = computed(() => {
  return (
    !!selectedFile.value &&
    !!fileId.value &&
    !!familyId.value &&
    !!category.value &&
    !!createdBy.value &&
    fileRules.every(rule => typeof rule === 'string' || rule(selectedFile.value as File) === true)
  );
});

const onFileChange = (event: Event) => {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    selectedFile.value = input.files[0];
  } else {
    selectedFile.value = null;
  }
};

const upload = async () => {
  if (selectedFile.value && isFormValid.value) {
    const metadata = {
      fileId: fileId.value,
      familyId: familyId.value,
      category: category.value,
      createdBy: createdBy.value,
    };
    await chunkStore.uploadFile(selectedFile.value, metadata);
  }
};

// Clear form after successful upload
watch(() => chunkStore.chunks, (newChunks) => {
  if (newChunks.length > 0 && !chunkStore.error) {
    selectedFile.value = null;
    fileId.value = '';
    familyId.value = '';
    category.value = '';
    createdBy.value = '';
  }
});
</script>
