<template>
  <VFileUpload
    v-model="selectedFile"
    :label="$t('chunkUpload.fileInputLabel')"
    :accept="acceptTypes"
    prepend-icon="mdi-paperclip"
    density="compact"
    show-size
    counter
    :rules="fileRules"
    @update:modelValue="onFileChange"
  />
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { VFileUpload } from 'vuetify/labs/VFileUpload';

const { t } = useI18n();
const emit = defineEmits(['file-selected', 'file-cleared']);
const props = defineProps({
  clearFile: {
    type: Boolean,
    default: false,
  },
});

const selectedFile = ref<File | undefined>(undefined);

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

const onFileChange = (files: File[]) => {
  selectedFile.value = files.length > 0 ? files[0] : undefined;
  emit('file-selected', selectedFile.value);
};

watch(() => props.clearFile, (newVal) => {
  if (newVal) {
    selectedFile.value = undefined;
    emit('file-cleared');
  }
});
</script>