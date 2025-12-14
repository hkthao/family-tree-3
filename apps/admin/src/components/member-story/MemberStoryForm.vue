<template>
  <v-container>
    <!-- Loading Indicator -->
    <div v-if="isLoading" class="my-4">
      <v-progress-linear color="primary" indeterminate></v-progress-linear>
    </div>

    <!-- NEW: File Upload Input (moved to top, always visible if not readonly) -->
    <v-row v-if="!readonly">
      <v-col cols="12">
        <v-file-input
          v-model="selectedFiles"
          :label="t('memberStory.form.uploadImagesLabel')"
          prepend-icon="mdi-camera"
          multiple
          accept="image/*"
          show-size
          counter
        ></v-file-input>
      </v-col>
    </v-row>

    <!-- NEW: Combined Carousel for existing and newly selected images -->
    <v-row v-if="combinedImageUrls.length > 0">
      <v-col cols="12">
        <v-carousel cycle hide-delimiter-background show-arrows="hover" :height="300" class="mb-4">
          <v-carousel-item v-for="(imageUrl, i) in combinedImageUrls" :key="i">
            <v-img :src="imageUrl" cover class="fill-height"></v-img>
          </v-carousel-item>
        </v-carousel>
      </v-col>
    </v-row>

    <!-- Member Selection -->
    <v-row>
      <v-col cols="12" sm="6">
        <FamilyAutocomplete :model-value="modelValue.familyId" :readonly="true" :label="t('memberStory.form.familyIdLabel')" />
      </v-col>
      <v-col cols="12" sm="6">
        <MemberAutocomplete :model-value="modelValue.memberId" :disabled="!modelValue.familyId"
          @update:modelValue="(newValue: string | null) => { updateModelValue({ memberId: newValue || '' }); }"
          :readonly="readonly" :family-id="modelValue.familyId" :label="t('memberStory.form.memberIdLabel')"
          :rules="[rules.memberId.required]" />
      </v-col>
    </v-row>

    <!-- New fields for Life Story -->
    <v-row>
      <v-col cols="12" sm="6">
        <v-text-field :model-value="modelValue.year"
          @update:modelValue="(newValue) => { updateModelValue({ year: parseInt(newValue) || null }); }"
          :label="t('memberStory.form.yearLabel')" type="number" :readonly="readonly"
          :rules="[rules.year.valid]"></v-text-field>
      </v-col>
      <v-col cols="12" sm="6">
        <v-select :model-value="modelValue.lifeStage"
          @update:modelValue="(newValue) => { updateModelValue({ lifeStage: newValue }); }"
          :label="t('memberStory.form.lifeStageLabel')" :items="lifeStageOptions" item-title="text" item-value="value"
          :readonly="readonly"></v-select>
      </v-col>
      <v-col cols="12">
        <v-text-field :model-value="modelValue.timeRangeDescription"
          @update:modelValue="(newValue) => { updateModelValue({ timeRangeDescription: newValue }); }"
          :label="t('memberStory.form.timeRangeDescriptionLabel')" :readonly="readonly"
          :rules="[rules.timeRangeDescription.maxLength]"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field :model-value="modelValue.location"
          @update:modelValue="(newValue) => { updateModelValue({ location: newValue }); }"
          :label="t('memberStory.form.locationLabel')" :readonly="readonly"
          :rules="[rules.location.maxLength]"></v-text-field>
      </v-col>

    </v-row>

    <!-- Title and Story -->
    <v-row>
      <v-col cols="12">
        <v-text-field :model-value="modelValue.title"
          @update:modelValue="(newValue) => { updateModelValue({ title: newValue }); }"
          :label="t('memberStory.storyEditor.title')" outlined class="mb-4" :rules="[rules.title.required]"
          :readonly="readonly" />
        <v-textarea :model-value="modelValue.story"
          @update:modelValue="(newValue) => { updateModelValue({ story: newValue }); }"
          :label="t('memberStory.storyEditor.storyContent')" outlined auto-grow :rules="[rules.story.required]"
          :readonly="readonly" />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { computed, ref, watch, onUnmounted } from 'vue'; // Added watch, onUnmounted
import { useI18n } from 'vue-i18n';
import type { MemberStoryDto } from '@/types/memberStory';
// Removed FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue'; // Import FamilyAutocomplete
import { useMemberStoryForm } from '@/composables'; // This composable needs to be adapted.
import { LifeStage } from '@/types/enums'; // Import enums

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

// Validation refs
const memberIdValid = ref(false);
const titleValid = ref(false);
const storyValid = ref(false);

const rules = {
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
  },
  location: {
    maxLength: (value: string | null) => !value || value.length <= 200 || t('common.validations.maxLength', { length: 200 }),
  },
};

const updateModelValue = (payload: Partial<MemberStoryDto>) => {
  const newModelValue = { ...props.modelValue, ...payload };
  emit('update:modelValue', newModelValue);

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

const formValid = computed(() => {
  return memberIdValid.value && titleValid.value && storyValid.value;
});

// New State for file input
const selectedFiles = ref<File[]>([]);
const temporaryLocalImageUrls = ref<string[]>([]);

// Combined image URLs for carousel
const combinedImageUrls = computed(() => {
  const existingImages = props.modelValue.memberStoryImages?.map(img => img.imageUrl).filter(url => url !== null && url !== undefined) as string[] || [];
  return [...existingImages, ...temporaryLocalImageUrls.value];
});

// Watch for selectedFiles changes and create local blob URLs
watch(selectedFiles, (newFiles) => {
  temporaryLocalImageUrls.value.forEach(url => URL.revokeObjectURL(url)); // Clean up old blobs
  temporaryLocalImageUrls.value = [];

  if (newFiles && newFiles.length > 0) {
    newFiles.forEach(file => {
      temporaryLocalImageUrls.value.push(URL.createObjectURL(file));
    });
    // Update modelValue with the first selected file's temporary URLs
    // This part assumes that modelValue can handle multiple temporary images,
    // which MemberStoryDto currently doesn't. Will pass the first one for now.
    updateModelValue({
      temporaryOriginalImageUrl: temporaryLocalImageUrls.value[0],
      temporaryResizedImageUrl: temporaryLocalImageUrls.value[0], // Assuming resized can be same as original for display
    });
  } else {
    updateModelValue({
      temporaryOriginalImageUrl: undefined,
      temporaryResizedImageUrl: undefined,
    });
  }
}, { deep: true }); // Use deep: true for watching array changes

// Cleanup blob URLs when component is unmounted
onUnmounted(() => {
  temporaryLocalImageUrls.value.forEach(url => URL.revokeObjectURL(url));
});


// Simplified useMemberStoryForm (no face detection logic)
const {
  isLoading,
  // handleFileUpload is removed from destructuring, as it's now handled locally via selectedFiles watch
} = useMemberStoryForm({
  modelValue: computed(() => props.modelValue),
  readonly: props.readonly,
  memberId: props.modelValue.memberId,
  familyId: props.modelValue.familyId,
  updateModelValue,
});

defineExpose({
  isValid: computed(() => formValid.value),
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