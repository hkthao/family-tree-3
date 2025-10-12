<template>
  <v-file-input v-model="selectedFile" :label="$t('chunkUpload.fileInputLabel')" accept=".pdf,.txt"
    prepend-icon="mdi-paperclip" show-size counter :rules="fileRules" @change="onFileChange"></v-file-input>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const emit = defineEmits(['file-selected']);

const selectedFile = ref<File | null>(null);

const fileRules = [
  (value: File) => !!value || t('chunkUpload.fileRequired'),
  (value: File) => {
    const allowedTypes = ['application/pdf', 'text/plain'];
    return allowedTypes.includes(value?.type) || t('chunkUpload.fileTypeInvalid');
  },
];

const onFileChange = (event: Event) => {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    selectedFile.value = input.files[0];
    emit('file-selected', selectedFile.value);
  } else {
    selectedFile.value = null;
    emit('file-selected', null);
  }
};
</script>
