<template>
  <v-card :elevation="0" data-testid="memory-item-detail-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memoryItem.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert type="error" :text="error?.message || t('memoryItem.detail.errorLoading')"></v-alert>
      </div>
      <div v-else-if="memoryItem">
        <PrivacyAlert :is-private="memoryItem.isPrivate" />

        <div v-if="memoryItem.memoryMedia && memoryItem.memoryMedia.length > 0">
          <v-carousel cycle :show-arrows="memoryItem.memoryMedia.length > 1" hide-delimiter-background height="500px">
            <v-carousel-item v-for="(media, index) in memoryItem.memoryMedia" :key="index">
              <div class="d-flex fill-height justify-center align-center carousel-content">
                <v-img
                  v-if="media.type === 'Image'"
                  :src="media.url"
                  class="carousel-image"
                  max-height="500px"
                  contain
                ></v-img>
                <video
                  v-else-if="media.type === 'Video'"
                  :src="media.url"
                  controls
                  autoplay
                  loop
                  class="carousel-video"
                  max-height="500px"
                ></video>
                <div v-else>
                  <p>{{ t('memoryItem.detail.unsupportedMediaType') }}</p>
                  <a :href="media.url" target="_blank">{{ media.url }}</a>
                </div>
              </div>
            </v-carousel-item>
          </v-carousel>
        </div>

        <MemoryItemForm :initial-memory-item-data="memoryItem" :family-id="props.familyId" :read-only="true" />
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <!-- Potentially an edit button here, but for now, just close -->
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import MemoryItemForm from '@/components/memory-item/MemoryItemForm.vue';
import { useMemoryItemDetail } from '@/composables';
import PrivacyAlert from '@/components/common/PrivacyAlert.vue'; // Import PrivacyAlert

const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
  memoryItemId: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close']);

const { t } = useI18n();

const { state: { memoryItem, isLoading, error }, actions: { closeView } } = useMemoryItemDetail({
  familyId: props.familyId,
  memoryItemId: props.memoryItemId,
  onClose: () => {
    emit('close');
  },
});
</script>

<style scoped></style>