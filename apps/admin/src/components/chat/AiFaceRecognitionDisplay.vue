<template>
  <!-- Face Recognition Display -->
  <template
    v-if="message.intent === 'IMAGE_RECOGNITION_PAGE' && message.faceDetectionResults && message.faceDetectionResults.length > 0">
    <div v-for="(detectionResult, index) in message.faceDetectionResults" :key="detectionResult.imageId || index">
      <h4 class="mt-4">{{ t('aiChat.faceRecognitionResult') }}</h4>
      <p v-if="detectionResult.originalImageUrl" class="text-caption">
        {{ t('aiChat.imageSource') }}: <a :href="detectionResult.originalImageUrl" target="_blank">{{
          detectionResult.originalImageUrl }}</a>
      </p>
      <FaceBoundingBoxViewer v-if="detectionResult.originalImageUrl" :imageSrc="detectionResult.originalImageUrl"
        :faces="detectionResult.detectedFaces" :selectable="false" class="mt-2" />
      <p v-else class="text-caption">{{ t('aiChat.noImageForDisplay') }}</p>

      <div class="mt-2" v-if="detectionResult.detectedFaces && detectionResult.detectedFaces.length > 0">
        <v-chip v-for="face in detectionResult.detectedFaces.filter(f => f.status === 'recognized' && f.memberName)"
          :key="face.id" class="ma-1" color="success" @click="openOriginalImage(detectionResult.originalImageUrl)">
          <v-avatar start size="24" v-if="face.thumbnail">
            <v-img :src="`data:image/jpeg;base64,${face.thumbnail}`"></v-img>
          </v-avatar>
          {{ face.memberName }}
        </v-chip>
      </div>
    </div>
  </template>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import type { AiChatMessage } from '@/types';
import { useI18n } from 'vue-i18n';
import FaceBoundingBoxViewer from '@/components/face/FaceBoundingBoxViewer.vue';

defineProps({
  message: {
    type: Object as PropType<AiChatMessage>,
    required: true,
  },
  familyId: { // familyId is passed but not directly used in this specific block, keeping for consistency if needed later
    type: String,
    required: true,
  },
});

const { t } = useI18n();

const openOriginalImage = (imageUrl: string | null | undefined) => {
  if (imageUrl) {
    window.open(imageUrl, '_blank');
  }
};
</script>