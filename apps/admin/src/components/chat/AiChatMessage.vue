<template>
  <v-avatar class="mr-1" size="36">
    <v-icon>mdi-robot-outline</v-icon>
  </v-avatar>
  <v-sheet class="ma-1 pa-2 text-wrap v-sheet-message" color="secondary" rounded="lg">
    <div class="message-content">
      {{ message.text }}
    </div>
    <template v-if="message.intent === 'RELATIONSHIP_LOOKUP_PAGE'">
      <v-btn class="mt-2" variant="outlined" size="small" append-icon="mdi-arrow-right-circle"
        @click="emit('open-relationship-detection', familyId)">
        {{ t('aiChat.determineRelationship') }}
      </v-btn>
    </template>

    <ChatGeneratedDataList
      v-if="message.generatedData && (message.generatedData.members.length > 0 || message.generatedData.events.length > 0)"
      :generatedData="message.generatedData" :familyId="familyId"
      @add-generated-member="(member: MemberDto) => emit('add-generated-member', member)"
      @add-generated-event="(event: EventDto) => emit('add-generated-event', event)" />

    <!-- NEW: Face Recognition Display -->
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
            :key="face.id" class="ma-1" color="success"
            @click="openOriginalImage(detectionResult.originalImageUrl)">
            <v-avatar start size="24" v-if="face.thumbnail">
              <v-img :src="`data:image/jpeg;base64,${face.thumbnail}`"></v-img>
            </v-avatar>
            {{ face.memberName }}
          </v-chip>
        </div>
      </div>
    </template>
    <!-- END NEW -->
  </v-sheet>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import type { AiChatMessage, EventDto, MemberDto } from '@/types';
import { useI18n } from 'vue-i18n';
import ChatGeneratedDataList from '@/components/chat/ChatGeneratedDataList.vue'; // New import
import FaceBoundingBoxViewer from '@/components/face/FaceBoundingBoxViewer.vue';

defineProps({
  message: {
    type: Object as PropType<AiChatMessage>,
    required: true,
  },
  familyId: {
    type: String,
    required: true,
  },
});

const emit = defineEmits([
  'open-relationship-detection',
  'add-generated-member',
  'add-generated-event',
]);
const { t } = useI18n();

const openOriginalImage = (imageUrl: string | null | undefined) => { // Accept string | null | undefined
  if (imageUrl) {
    window.open(imageUrl, '_blank');
  }
};
</script>

<style scoped>
.message-content {
  white-space: pre-wrap;
  word-break: break-word;
}

.v-sheet-message {
  max-width: 89%;
}
</style>
