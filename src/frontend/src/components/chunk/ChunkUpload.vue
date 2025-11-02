<template>
  <v-file-upload :label="$t('chunkUpload.fileInputLabel')" :accept="acceptTypes"  :rules="fileRules"
    @update:modelValue="onFileChange" clearable />
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const emit = defineEmits(['file-selected', 'file-cleared']);


const acceptTypes = computed(() => '.pdf,.txt,.md');

const fileRules = [
  (value: File | undefined) => !!value || t('chunkUpload.fileRequired'),
  (value: File | undefined) => {
    if (!value) return t('chunkUpload.fileRequired');
    const allowedTypes = ['application/pdf', 'text/plain', 'text/markdown'];
    const allowedExtensions = ['.pdf', '.txt', '.md'];
    const fileExtension = value?.name ? '.' + value.name.split('.').pop()?.toLowerCase() : '';
    return (allowedTypes.includes(value?.type) || allowedExtensions.includes(fileExtension)) || t('chunkUpload.fileTypeInvalid');
  },
];

const onFileChange = (file: File[]) => {
  emit('file-selected', file);
};

</script>