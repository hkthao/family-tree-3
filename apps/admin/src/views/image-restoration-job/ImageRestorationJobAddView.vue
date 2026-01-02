<template>
  <v-card :elevation="0" data-testid="image-restoration-job-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('imageRestorationJob.form.addTitle') }}</span>
    </v-card-title>
    <v-card-text>
      <ImageRestorationJobForm ref="imageRestorationJobFormRef" :family-id="familyId" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm"
        :disabled="isAddingImageRestorationJob">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" data-testid="button-save" @click="handleAddItem"
        :loading="isAddingImageRestorationJob" :disabled="isAddingImageRestorationJob">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ImageRestorationJobForm from '@/components/image-restoration-job/ImageRestorationJobForm.vue';
import { useImageRestorationJobAdd } from '@/composables';
import { type IImageRestorationJobFormInstance } from '@/components/image-restoration-job/ImageRestorationJobForm.vue';

const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
});

const imageRestorationJobFormRef: Ref<IImageRestorationJobFormInstance | null> = ref(null);
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();

const {
  state: { isAddingImageRestorationJob },
  actions: { handleAddItem, closeForm },
} = useImageRestorationJobAdd({
  familyId: props.familyId,
  onSaveSuccess: () => {
    emit('close');
    emit('saved');
  },
  onCancel: () => {
    emit('close');
  },
  formRef: imageRestorationJobFormRef,
});
</script>

<style scoped></style>