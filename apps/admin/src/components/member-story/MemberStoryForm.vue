<template>
  <v-container>
    <!-- Member Selection -->
    <div v-if="isLoading" class="overlay">
      <v-progress-circular color="primary" indeterminate></v-progress-circular>
    </div>
    <v-row>
      <v-col cols="12">
        <FamilyAutocomplete :model-value="modelValue.familyId"
          @update:modelValue="(newValue: string | null) => { updateModelValue({ familyId: newValue || '' }); }"
          :readonly="readonly" :label="t('memberStory.form.familyIdLabel')" :rules="[rules.familyId.required]" />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <MemberAutocomplete :model-value="modelValue.memberId" :disabled="!modelValue.familyId"
          @update:modelValue="(newValue: string | null) => { updateModelValue({ memberId: newValue || '' }); }"
          :readonly="readonly" :family-id="modelValue.familyId" :label="t('memberStory.form.memberIdLabel')"
          :rules="[rules.memberId.required]" />
      </v-col>
    </v-row>
    <!-- Photo Upload Input -->
    <v-row v-if="modelValue.memberId">
      <v-col cols="12">
        <FaceUploadInput @file-uploaded="handleFileUpload" :readonly="readonly" />
      </v-col>
      <!-- Face Detection and Selection -->
      <v-col cols="12">
        <div v-if="hasUploadedImage">
          <div v-if="modelValue.detectedFaces && modelValue.detectedFaces.length > 0">
            <FaceBoundingBoxViewer :image-src="modelValue.originalImageUrl!" :faces="modelValue.detectedFaces"
              selectable @face-selected="openSelectMemberDialog" />
            <FaceDetectionSidebar :faces="modelValue.detectedFaces" @face-selected="openSelectMemberDialog"
              @remove-face="handleRemoveFace" />
          </div>
          <v-alert v-else type="info">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>
        </div>
        <v-alert v-else type="info">{{
          t('memberStory.faceRecognition.uploadPrompt') }}</v-alert>
      </v-col>
    </v-row>

    <!-- Raw Input & Story Style -->
    <v-row v-if="hasUploadedImage">
      <v-col cols="12">
        <h4>{{ t('memberStory.create.storyStyle.question') }}</h4>
        <v-chip-group :model-value="modelValue.storyStyle"
          @update:model-value="(newValue) => updateModelValue({ storyStyle: newValue })" color="primary" mandatory
          column :disabled="readonly">
          <v-chip v-for="style in storyStyles" :key="style.value" :value="style.value" filter variant="tonal">
            {{ style.text }}
          </v-chip>
        </v-chip-group>
      </v-col>
    </v-row>

    <!-- Perspective -->
    <v-row v-if="hasUploadedImage">
      <v-col cols="12">
        <h4>{{ t('memberStory.create.perspective.question') }}</h4>
        <v-chip-group :model-value="modelValue.perspective"
          @update:model-value="(newValue) => updateModelValue({ perspective: newValue })" color="primary" mandatory
          column :disabled="readonly">
          <v-chip :value="aiPerspectiveSuggestions[0].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[0].text }}
          </v-chip>
          <v-chip :value="aiPerspectiveSuggestions[1].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[1].text }}
          </v-chip>
          <v-chip :value="aiPerspectiveSuggestions[2].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[2].text }}
          </v-chip>
        </v-chip-group>
      </v-col>
    </v-row>

    <v-row v-if="hasUploadedImage">
      <v-col cols="12">
        <v-textarea :model-value="modelValue.rawInput" :rows="2"
          @update:model-value="(newValue) => { updateModelValue({ rawInput: newValue }); }"
          :label="t('memberStory.create.rawInputPlaceholder')" :readonly="readonly" auto-grow />
      </v-col>
      <v-col cols="12">
        <v-btn color="primary" :disabled="readonly || generatingStory || isLoading || !canGenerateStory"
          :loading="generatingStory" @click="generateStory">
          {{ t('memberStory.create.generateStoryButton') }}
        </v-btn>
        <v-alert type="info" class="mt-4">
          {{ t('memberStory.create.aiConsentInfo') }}
        </v-alert>
      </v-col>
    </v-row>

    <!-- Title and Story -->
    <v-row v-if="hasUploadedImage">

      <v-col cols="12">
        <v-text-field :model-value="modelValue.title"
          @update:model-value="(newValue) => { updateModelValue({ title: newValue }); }"
          :label="t('memberStory.storyEditor.title')" outlined class="mb-4" :rules="[rules.title.required]" />
        <v-textarea :model-value="modelValue.story"
          @update:model-value="(newValue) => { updateModelValue({ story: newValue }); }"
          :label="t('memberStory.storyEditor.storyContent')" outlined auto-grow :rules="[rules.story.required]" />
      </v-col>
    </v-row>

    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" :family-id="modelValue.familyId"
      :show-relation-prompt-field="true" :disable-save-validation="true" />
  </v-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberStoryDto } from '@/types/memberStory';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue'; // Import FamilyAutocomplete
import { useMemberStoryForm } from '@/composables/useMemberStoryForm';

const props = defineProps<{
  modelValue: MemberStoryDto;
  readonly?: boolean;
}>();
const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles', 'story-generated']);
const { t } = useI18n();

// Validation refs
const familyIdValid = ref(false); // Thêm validation ref cho familyId
const memberIdValid = ref(false);
const titleValid = ref(false);
const storyValid = ref(false);

const rules = {
  familyId: { // Thêm rule cho familyId
    required: (value: string | null) => !!value || t('common.validations.required'),
  },
  memberId: {
    required: (value: string | null) => !!value || t('common.validations.required'),
  },
  title: {
    required: (value: string | null) => !!value || t('common.validations.required'),
  },
  story: {
    required: (value: string | null) => !!value || t('common.validations.required'),
  },
};

const updateModelValue = (payload: Partial<MemberStoryDto>) => {
  const newModelValue = { ...props.modelValue, ...payload };
  emit('update:modelValue', newModelValue);

  // Cập nhật trạng thái valid cho từng trường dựa trên payload
  if (payload.familyId !== undefined) {
    familyIdValid.value = !!rules.familyId.required(newModelValue.familyId ?? null);
  }
  if (payload.memberId !== undefined) {
    memberIdValid.value = !!rules.memberId.required(newModelValue.memberId ?? null);
  }
  if (payload.title !== undefined) {
    titleValid.value = !!rules.title.required(newModelValue.title ?? null);
  }
  if (payload.story !== undefined) {
    storyValid.value = !!rules.story.required(newModelValue.story ?? null);
  }
};
const onStoryGenerated = (payload: { story: string | null; title: string | null }) => {
  emit('story-generated', payload);
};

const formValid = computed(() => {
  return familyIdValid.value && memberIdValid.value && titleValid.value && storyValid.value;
});

const {
  showSelectMemberDialog,
  faceToLabel,
  aiPerspectiveSuggestions,
  storyStyles,
  generatedTitle,
  generatingStory,
  storyEditorValid,
  hasUploadedImage,
  isLoading,
  canGenerateStory,
  generateStory,
  handleFileUpload,
  openSelectMemberDialog,
  handleLabelFaceAndCloseDialog,
  handleRemoveFace,
  memberStoryStoreFaceRecognition,
} = useMemberStoryForm({
  modelValue: computed(() => props.modelValue),
  readonly: props.readonly,
  memberId: props.modelValue.memberId,
  familyId: props.modelValue.familyId,
  updateModelValue,
  onStoryGenerated,
});

defineExpose({
  isValid: computed(() => formValid.value),
  memoryFaceStore: memberStoryStoreFaceRecognition,
  generateStory,
  generatedTitle,
  storyEditorValid,
});
</script>

<style scoped>
.overlay {
  display: flex;
  flex-direction: column;
  position: absolute;
  z-index: 10;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  cursor: wait;
}
</style>
