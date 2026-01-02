<template>
  <v-navigation-drawer :model-value="modelValue" @update:model-value="emit('update:modelValue', $event)" location="right"
    temporary width="500">
    <template #prepend>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('imageRestorationJob.detailDrawer.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn icon @click="closeDrawer">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-toolbar>
    </template>

    <v-card flat class="pa-4">
      <v-card-text v-if="job">
        <v-row>
          <v-col cols="12">
            <h3 class="mb-2">{{ t('imageRestorationJob.detailDrawer.originalImage') }}</h3>
            <ImageEnlarger :src="job.originalImageUrl" :alt="t('imageRestorationJob.detailDrawer.originalImage')"
              thumbnail-height="200" thumbnail-width="auto">
            </ImageEnlarger>
          </v-col>

          <v-col cols="12">
            <h3 class="mb-2">{{ t('imageRestorationJob.detailDrawer.restoredImage') }}</h3>
            <ImageEnlarger v-if="job.restoredImageUrl" :src="job.restoredImageUrl"
              :alt="t('imageRestorationJob.detailDrawer.restoredImage')" thumbnail-height="200" thumbnail-width="auto">
            </ImageEnlarger>
            <span v-else>{{ t('common.na') }}</span>
          </v-col>

          <v-col cols="12">
            <v-list dense>
              <v-list-item>
                <v-list-item-title>{{ t('imageRestorationJob.detailDrawer.status') }}:</v-list-item-title>
                <v-list-item-subtitle>
                  <v-chip :color="getStatusColor(job.status)" small>
                    {{ job.status !== undefined ? t(`imageRestorationJob.status.${RestorationStatus[job.status].toLowerCase()}`) : '' }}
                  </v-chip>
                </v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>{{ t('imageRestorationJob.detailDrawer.created') }}:</v-list-item-title>
                <v-list-item-subtitle>{{ formatDate(job.created) }}</v-list-item-subtitle>
              </v-list-item>
              <v-list-item v-if="job.lastModified">
                <v-list-item-title>{{ t('imageRestorationJob.detailDrawer.lastModified') }}:</v-list-item-title>
                <v-list-item-subtitle>{{ formatDate(job.lastModified) }}</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-text v-else>
        <v-alert type="info">{{ t('imageRestorationJob.detailDrawer.noJobSelected') }}</v-alert>
      </v-card-text>
    </v-card>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { type ImageRestorationJobDto, RestorationStatus } from '@/types';
import { formatDate } from '@/utils/format.utils';
import ImageEnlarger from '@/components/common/ImageEnlarger.vue';

interface ImageRestorationJobDetailDrawerProps {
  modelValue: boolean;
  job: ImageRestorationJobDto | null;
}

const props = defineProps<ImageRestorationJobDetailDrawerProps>();
const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

const closeDrawer = () => {
  emit('update:modelValue', false);
};

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