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
          <v-col cols="12" v-if="imageRestorationJob.restoredImageUrl">
            <v-card outlined class="pa-4">
              <v-card-title class="text-h6">{{ t('imageRestorationJob.detail.jobDetails') }}</v-card-title>
              <v-list dense>
                <v-list-item>
                  <v-list-item-title>{{ t('imageRestorationJob.detailDrawer.status') }}:</v-list-item-title>
                  <v-list-item-subtitle>
                    <v-chip :color="getStatusColor(imageRestorationJob.status)" small>
                      {{ imageRestorationJob.status !== undefined ? t(`imageRestorationJob.status.${RestorationStatus[imageRestorationJob.status].toLowerCase()}`) : '' }}
                    </v-chip>
                  </v-list-item-subtitle>
                </v-list-item>
                <v-list-item>
                  <v-list-item-title>{{ t('imageRestorationJob.detailDrawer.created') }}:</v-list-item-title>
                  <v-list-item-subtitle>{{ formatDate(imageRestorationJob.created) }}</v-list-item-subtitle>
                </v-list-item>
                <v-list-item v-if="imageRestorationJob.lastModified">
                  <v-list-item-title>{{ t('imageRestorationJob.detailDrawer.lastModified') }}:</v-list-item-title>
                  <v-list-item-subtitle>{{ formatDate(imageRestorationJob.lastModified) }}</v-list-item-subtitle>
                </v-list-item>
              </v-list>
            </v-card>
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
import { RestorationStatus } from '@/types';
import { formatDate } from '@/utils/format.utils';

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

const getStatusColor = (status: RestorationStatus) => {
  switch (status) {
    case RestorationStatus.Processing:
      return 'blue';
    case RestorationStatus.Completed:
      return 'green';
    case RestorationStatus.Failed:
      return 'red';
    default:
      return 'grey';
  }
};
</script>

<style scoped></style>