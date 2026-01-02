<template>
  <v-card :elevation="0" data-testid="image-restoration-job-detail-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('imageRestorationJob.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoading && !imageRestorationJob && !error">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert type="error" :text="error?.message || t('imageRestorationJob.messages.notFound')"></v-alert>
      </div>
      <div v-else-if="imageRestorationJob">
        <v-row class="justify-center">
          <v-col cols="12">
            <div v-if="imageRestorationJob.originalImageUrl">
              <ImageComparisonSlider
                :before-image-src="imageRestorationJob.originalImageUrl"
                :after-image-src="imageRestorationJob.restoredImageUrl || imageRestorationJob.originalImageUrl"
              />
            </div>
            <v-alert v-else type="info" class="mt-4">
              {{ t('imageRestorationJob.detail.noOriginalImage') }}
            </v-alert>
          </v-col>
        </v-row>
      </div>
      <div v-else>
        <v-alert type="info" class="mt-4">
          {{ t('imageRestorationJob.detail.noJobSelected') }}
        </v-alert>
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
import ImageComparisonSlider from '@/components/image-restoration-job/ImageComparisonSlider.vue';
import { useImageRestorationJobDetail } from '@/composables';

const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
  id: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close']);

const { t } = useI18n();

const { state: { imageRestorationJob, isLoading, error }, actions: { closeView } } = useImageRestorationJobDetail({
  familyId: computed(() => props.familyId),
  id: computed(() => props.id),
  onClose: () => {
    emit('close');
  },
});
</script>

<style scoped></style>