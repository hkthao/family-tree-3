<template>
  <VFileUpload
    :label="label || t('face.uploadInput.selectImage')"
    :accept="accept"
    :multiple="multiple"
    prepend-icon="mdi-camera"
    counter
    show-size
    clearable 
    density="compact" 
    @change="handleFileChange"
  ></VFileUpload>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { VFileUpload } from 'vuetify/labs/VFileUpload'; // Correct import

const { t } = useI18n();

const props = defineProps({
  label: String,
  accept: { type: String, default: 'image/*' },
  multiple: { type: Boolean, default: false },
});

const emit = defineEmits(['file-uploaded']);

const handleFileChange = (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    if (props.multiple) {
      emit('file-uploaded', Array.from(target.files));
    } else {
      emit('file-uploaded', target.files[0]);
    }
  }
};
</script>

<style scoped>
/* No custom styles needed for VFileUpload */
</style>
