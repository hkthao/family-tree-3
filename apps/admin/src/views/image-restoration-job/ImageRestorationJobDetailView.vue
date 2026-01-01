<template>
  <v-card :elevation="0" data-testid="image-restoration-job-detail-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('imageRestorationJob.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert type="error" :text="error?.message || t('imageRestorationJob.messages.notFound')"></v-alert>
      </div>
      <div v-else-if="imageRestorationJob">
        <ImageRestorationJobForm :initial-data="imageRestorationJob" :family-id="props.familyId" :read-only="true" />
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { type PropType, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import ImageRestorationJobForm from '@/components/image-restoration-job/ImageRestorationJobForm.vue';
import { useImageRestorationJobDetail } from '@/composables';

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

const emit = defineEmits(['close']);

const { t } = useI18n();

const { state: { imageRestorationJob, isLoading, error }, actions: { closeView } } = useImageRestorationJobDetail({
  familyId: computed(() => props.familyId),
  imageRestorationJobId: computed(() => props.imageRestorationJobId),
  onClose: () => {
    emit('close');
  },
});
</script>

<style scoped></style>