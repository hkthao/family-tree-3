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
        <v-carousel v-if="modelValue.memberStoryImages && modelValue.memberStoryImages.length > 0" cycle hide-delimiter-background show-arrows="hover" :height="300" class="mb-4">
          <v-carousel-item v-for="(image, i) in modelValue.memberStoryImages" :key="i">
            <v-img :src="image.imageUrl" cover class="fill-height"></v-img>
          </v-carousel-item>
        </v-carousel>
        <FaceUploadInput @file-uploaded="handleFileUpload" :readonly="readonly" />
      </v-col>
      <!-- Face Detection and Selection -->
      <v-col cols="12">
        <div v-if="hasUploadedImage || (modelValue.memberStoryImages && modelValue.memberStoryImages.length > 0)">
          <div v-if="modelValue.detectedFaces && modelValue.detectedFaces.length > 0">
            <FaceBoundingBoxViewer :image-src="modelValue.originalImageUrl || modelValue.memberStoryImages?.[0]?.imageUrl || ''" :faces="modelValue.detectedFaces"
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

    <!-- New fields for Life Story -->
    <v-row>
      <v-col cols="12" sm="6">
        <v-text-field
          :model-value="modelValue.year"
          @update:model-value="(newValue) => { updateModelValue({ year: parseInt(newValue) || null }); }"
          :label="t('memberStory.form.yearLabel')"
          type="number"
          :readonly="readonly"
          :rules="[rules.year.valid]"
        ></v-text-field>
      </v-col>
      <v-col cols="12" sm="6">
        <v-checkbox
          :model-value="modelValue.isYearEstimated"
          @update:model-value="(newValue) => { updateModelValue({ isYearEstimated: newValue }); }"
          :label="t('memberStory.form.isYearEstimatedLabel')"
          :readonly="readonly"
          class="mt-0 pt-0"
        ></v-checkbox>
      </v-col>
      <v-col cols="12">
        <v-text-field
          :model-value="modelValue.timeRangeDescription"
          @update:model-value="(newValue) => { updateModelValue({ timeRangeDescription: newValue }); }"
          :label="t('memberStory.form.timeRangeDescriptionLabel')"
          :readonly="readonly"
          :rules="[rules.timeRangeDescription.maxLength]"
        ></v-text-field>
      </v-col>
      <v-col cols="12" sm="6">
        <v-select
          :model-value="modelValue.lifeStage"
          @update:model-value="(newValue) => { updateModelValue({ lifeStage: newValue }); }"
          :label="t('memberStory.form.lifeStageLabel')"
          :items="lifeStageOptions"
          item-title="text"
          item-value="value"
          :readonly="readonly"
          :rules="[rules.lifeStage.required]"
        ></v-select>
      </v-col>
      <v-col cols="12" sm="6">
        <v-select
          :model-value="modelValue.certaintyLevel"
          @update:model-value="(newValue) => { updateModelValue({ certaintyLevel: newValue }); }"
          :label="t('memberStory.form.certaintyLevelLabel')"
          :items="certaintyLevelOptions"
          item-title="text"
          item-value="value"
          :readonly="readonly"
          :rules="[rules.certaintyLevel.required]"
        ></v-select>
      </v-col>
      <v-col cols="12">
        <v-text-field
          :model-value="modelValue.location"
          @update:model-value="(newValue) => { updateModelValue({ location: newValue }); }"
          :label="t('memberStory.form.locationLabel')"
          :readonly="readonly"
          :rules="[rules.location.maxLength]"
        ></v-text-field>
      </v-col>
      <v-col cols="12">
        <MemberAutocomplete
          :model-value="modelValue.storytellerId"
          @update:modelValue="(newValue: string | null) => { updateModelValue({ storytellerId: newValue || null }); }"
          :readonly="readonly"
          :family-id="modelValue.familyId"
          :label="t('memberStory.form.storytellerLabel')"
        />
      </v-col>
    </v-row>

    <!-- Title and Story -->
    <v-row>
      <v-col cols="12">
        <v-text-field :model-value="modelValue.title"
          @update:model-value="(newValue) => { updateModelValue({ title: newValue }); }"
          :label="t('memberStory.storyEditor.title')" outlined class="mb-4" :rules="[rules.title.required]"
          :readonly="readonly" />
        <v-textarea :model-value="modelValue.story"
          @update:model-value="(newValue) => { updateModelValue({ story: newValue }); }"
          :label="t('memberStory.storyEditor.storyContent')" outlined auto-grow :rules="[rules.story.required]"
          :readonly="readonly" />
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
import { CertaintyLevel, LifeStage } from '@/types/enums'; // Import enums

const props = defineProps<{
  modelValue: MemberStoryDto;
  readonly?: boolean;
}>();
const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles']);
const { t } = useI18n();

// Options for v-select
const lifeStageOptions = computed(() => [
  { value: LifeStage.Childhood, text: t('lifeStage.childhood') },
  { value: LifeStage.Adulthood, text: t('lifeStage.adulthood') },
  { value: LifeStage.StartingAFamily, text: t('lifeStage.startingAFamily') },
  { value: LifeStage.SignificantEvents, text: t('lifeStage.significantEvents') },
  { value: LifeStage.OldAge, text: t('lifeStage.oldAge') },
  { value: LifeStage.Deceased, text: t('lifeStage.deceased') },
]);

const certaintyLevelOptions = computed(() => [
  { value: CertaintyLevel.Sure, text: t('certaintyLevel.sure') },
  { value: CertaintyLevel.Estimated, text: t('certaintyLevel.estimated') },
]);

// Validation refs
const familyIdValid = ref(false);
const memberIdValid = ref(false);
const titleValid = ref(false);
const storyValid = ref(false);
const lifeStageValid = ref(false);
const certaintyLevelValid = ref(false);

const rules = {
  familyId: {
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
  year: {
    valid: (value: number | null) => !value || (value >= 1000 && value <= new Date().getFullYear() + 1) || t('memberStory.form.yearRangeValidation'),
  },
  timeRangeDescription: {
    maxLength: (value: string | null) => !value || value.length <= 100 || t('common.validations.maxLength', { length: 100 }),
  },
  lifeStage: {
    required: (value: LifeStage | null) => value !== null || t('common.validations.required'),
  },
  location: {
    maxLength: (value: string | null) => !value || value.length <= 200 || t('common.validations.maxLength', { length: 200 }),
  },
  certaintyLevel: {
    required: (value: CertaintyLevel | null) => value !== null || t('common.validations.required'),
  },
};

const updateModelValue = (payload: Partial<MemberStoryDto>) => {
  const newModelValue = { ...props.modelValue, ...payload };
  emit('update:modelValue', newModelValue);

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
  if (payload.lifeStage !== undefined) {
    lifeStageValid.value = !!rules.lifeStage.required(newModelValue.lifeStage ?? null);
  }
  if (payload.certaintyLevel !== undefined) {
    certaintyLevelValid.value = !!rules.certaintyLevel.required(newModelValue.certaintyLevel ?? null);
  }
};

const formValid = computed(() => {
  return familyIdValid.value && memberIdValid.value && titleValid.value && storyValid.value && lifeStageValid.value && certaintyLevelValid.value;
});

const {
  showSelectMemberDialog,
  faceToLabel,
  hasUploadedImage,
  isLoading,
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
});

defineExpose({
  isValid: computed(() => formValid.value),
  memoryFaceStore: memberStoryStoreFaceRecognition,
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
