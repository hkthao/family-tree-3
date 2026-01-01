<template>
  <v-card :elevation="0" data-testid="image-restoration-job-edit-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('imageRestorationJob.form.editTitle') }}</span>
    </v-card-title>
    <v-card-text>
      <ImageRestorationJobForm
        ref="imageRestorationJobFormRef"
        v-if="imageRestorationJob"
        :initial-data="imageRestorationJob"
        :family-id="props.familyId"
        :read-only="false"
      />
      <v-progress-circular v-else indeterminate color="primary"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn
        color="grey"
        data-testid="button-cancel"
        @click="closeForm"
        :disabled="isLoading || isUpdatingImageRestorationJob"
      >{{ t('common.cancel') }}</v-btn>
      <v-btn
        color="primary"
        data-testid="button-save"
        @click="handleUpdateItem"
        :loading="isUpdatingImageRestorationJob"
        :disabled="isLoading || isUpdatingImageRestorationJob"
      >{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import ImageRestorationJobForm from '@/components/image-restoration-job/ImageRestorationJobForm.vue';
import { useImageRestorationJobEdit } from '@/composables';
import { type IImageRestorationJobFormInstance } from '@/components/image-restoration-job/ImageRestorationJobForm.vue';

const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
  imageRestorationJobId: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close', 'saved']);

const imageRestorationJobFormRef: Ref<IImageRestorationJobFormInstance | null> = ref(null);

const { t } = useI18n();

const {
  state: { imageRestorationJob, isLoading, isUpdatingImageRestorationJob },
  actions: { handleUpdateItem, closeForm },
} = useImageRestorationJobEdit({
  familyId: computed(() => props.familyId),
  imageRestorationJobId: computed(() => props.imageRestorationJobId),
  onSaveSuccess: () => {
    emit('saved');
  },
  onCancel: () => {
    emit('close');
  },
  formRef: imageRestorationJobFormRef,
});
</script>

<style scoped></style>