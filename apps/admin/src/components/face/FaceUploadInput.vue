<template>
  <VFileUpload
    ref="fileUploadRef"
    v-model="files"
    :label="label || t('face.uploadInput.selectImage')"
    :accept="accept"
    :multiple="multiple"
    prepend-icon="mdi-camera"
    counter
    show-size
    clearable
    @click:clear="reset"
  ></VFileUpload>
</template>

<script setup lang="ts">
import { VFileUpload } from 'vuetify/labs/VFileUpload';
import { useFaceUploadInput } from '@/composables';

const props = defineProps({
  label: String,
  accept: { type: String, default: 'image/*' },
  multiple: { type: Boolean, default: false },
});

const emit = defineEmits(['file-uploaded']);

const {
  t,
  files,
  reset,
} = useFaceUploadInput(props, emit);

defineExpose({
  reset,
});
</script>

<style scoped>
/* No custom styles needed for VFileUpload */
</style>
