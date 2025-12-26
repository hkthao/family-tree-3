<template>
  <v-avatar class="mr-1" size="36">
    <v-icon>mdi-robot-outline</v-icon>
  </v-avatar>
  <v-sheet class="ma-1 pa-2 text-wrap" color="secondary" rounded="lg">
    <div class="message-content">
      {{ message.text }}
    </div>
    <template v-if="message.intent === 'RELATIONSHIP_LOOKUP_PAGE'">
      <v-btn
        class="mt-2"
        variant="outlined"
        size="small"
        append-icon="mdi-arrow-right-circle"
        @click="emit('open-relationship-detection', familyId)"
      >
        {{ t('aiChat.determineRelationship') }}
      </v-btn>
    </template>

    <ChatGeneratedDataList
      v-if="message.generatedData && (message.generatedData.members.length > 0 || message.generatedData.events.length > 0)"
      :generatedData="message.generatedData"
      :familyId="familyId"
      @add-generated-member="(member: MemberDto) => emit('add-generated-member', member)"
      @add-generated-event="(event: EventDto) => emit('add-generated-event', event)"
    />

    <!-- NEW: Face Recognition Display -->
    <template v-if="message.intent === 'IMAGE_RECOGNITION_PAGE' && message.faceDetectionResults && message.faceDetectionResults.length > 0">
      <div v-for="(detectionResult, index) in message.faceDetectionResults" :key="detectionResult.imageId || index">
        <h4 class="mt-4">{{ t('aiChat.faceRecognitionResult') }}</h4>
        <p v-if="detectionResult.originalImageUrl" class="text-caption">
          {{ t('aiChat.imageSource') }}: <a :href="detectionResult.originalImageUrl" target="_blank">{{ detectionResult.originalImageUrl }}</a>
        </p>
        <FaceBoundingBoxViewer
          v-if="detectionResult.resizedImageUrl"
          :imageSrc="detectionResult.resizedImageUrl"
          :faces="detectionResult.detectedFaces"
          :selectable="false"
          class="mt-2"
          style="max-width: 400px; height: auto;"
        />
        <p v-else class="text-caption">{{ t('aiChat.noImageForDisplay') }}</p>
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
import type { DetectedFace as ViewerDetectedFace, FaceStatus, BoundingBox } from '@/types/memberFace.d'; // Import consistent types

// Define a type for the backend's DetectedFaceDto for clear mapping
interface BackendDetectedFaceDto {
  faceId?: string;
  memberId?: string;
  memberName?: string;
  boundingBox: BoundingBox;
  confidence: number;
  status: string; // Backend's enum comes as string
}

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

// Helper function to map backend's DetectedFaceDto to FaceBoundingBoxViewer's expected DetectedFace
/* eslint-disable-next-line @typescript-eslint/no-unused-vars */
const mapFacesForViewer = (faces: BackendDetectedFaceDto[]): ViewerDetectedFace[] => {
  return faces.map(face => ({
    id: face.faceId || crypto.randomUUID(), // Ensure id is always a string
    boundingBox: face.boundingBox,
    confidence: face.confidence,
    memberId: face.memberId || null,
    memberName: face.memberName,
    status: face.status.toLowerCase() as FaceStatus, // Cast to the expected FaceStatus literal type
    embedding: null, // Provide null as embedding is not provided by ChatResponse
  }));
};
</script>

<style scoped>
.message-content {
  white-space: pre-wrap; /* Preserves whitespace and wraps text */
  word-break: break-word; /* Ensures long words break to prevent overflow */
}
</style>
