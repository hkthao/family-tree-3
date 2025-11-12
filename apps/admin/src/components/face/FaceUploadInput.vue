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
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { VFileUpload } from 'vuetify/labs/VFileUpload';

const { t } = useI18n();

const props = defineProps({
  label: String,
  accept: { type: String, default: 'image/*' },
  multiple: { type: Boolean, default: false },
});

const emit = defineEmits(['file-uploaded']);

const fileUploadRef = ref<InstanceType<typeof VFileUpload> | null>(null);
const files = ref<File[]>([]);

watch(files, (newFiles) => {
  if (newFiles && newFiles.length > 0) {
    if (props.multiple) {
      emit('file-uploaded', newFiles);
    } else {
      emit('file-uploaded', newFiles[0]);
    }
  } else {
    emit('file-uploaded', null);
  }
});

const reset = () => {
  files.value = [];
  // The watch on `files` will emit the null value
};

defineExpose({
  reset,
});
</script>

<style scoped>
/* No custom styles needed for VFileUpload */
</style>
