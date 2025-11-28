<template>
  <v-container>
    <!-- Member Selection -->
    <v-row>
      <v-col cols="12">
        <MemberAutocomplete :model-value="modelValue.memberId"
          @update:modelValue="(newValue: string | null) => { updateModelValue({ memberId: newValue }); v$.memberId.$touch(); }"
          :readonly="readonly" :family-id="familyId" :label="t('memberStory.form.memberIdLabel')"
          :error-messages="v$.memberId.$errors.map(e => e.$message as string)"
          :error="v$.memberId.$invalid && v$.memberId.$dirty"
        />
      </v-col>
    </v-row>
    <!-- Photo Upload Input -->
    <v-row>
      <v-col v-if="isLoading" cols="12">
        <v-progress-linear indeterminate color="primary"></v-progress-linear>
      </v-col>
      <v-col cols="12">
        <FaceUploadInput @file-uploaded="handleFileUpload" :readonly="readonly" />
      </v-col>
    </v-row>
    <!-- Face Detection and Selection -->
    <v-row>
      <v-col cols="12">
        <div v-if="hasUploadedImage && !isLoading">
          <div v-if="modelValue.faces && modelValue.faces.length > 0">
            <FaceBoundingBoxViewer :image-src="modelValue.photoUrl!" :faces="modelValue.faces" selectable
              @face-selected="openSelectMemberDialog" />
            <FaceDetectionSidebar :faces="modelValue.faces" @face-selected="openSelectMemberDialog"
              @remove-face="handleRemoveFace" />
          </div>
          <v-alert v-else type="info">{{ t('memberStory.faceRecognition.noFacesDetected') }}</v-alert>
        </div>
        <v-alert v-else type="info">{{
          t('memberStory.faceRecognition.uploadPrompt') }}</v-alert>
      </v-col>
    </v-row>

    <!-- Raw Input & Story Style -->
    <v-row v-if="hasUploadedImage && !isLoading">
      <v-col cols="12">
        <h4>{{ t('memberStory.create.rawInputPlaceholder') }}</h4>
        <v-textarea class="mt-4" :model-value="modelValue.rawInput" :rows="2"
          @update:model-value="(newValue) => { updateModelValue({ rawInput: newValue }); v$.rawInput.$touch(); }"
          :label="t('memberStory.create.rawInputPlaceholder')" :readonly="readonly" auto-grow
          :error-messages="v$.rawInput.$errors.map(e => e.$message as string)"
          :error="v$.rawInput.$invalid && v$.rawInput.$dirty"
        ></v-textarea>
      </v-col>
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
    <v-row v-if="hasUploadedImage && !isLoading">
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

    <v-row v-if="hasUploadedImage && !isLoading">
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
    <v-row v-if="hasUploadedImage && !isLoading">
      <v-col cols="12">
        <v-text-field :model-value="modelValue.title"
          @update:model-value="(newValue) => { updateModelValue({ title: newValue }); v$.title.$touch(); }"
          :label="t('memberStory.storyEditor.title')" outlined
          class="mb-4"
          :error-messages="v$.title.$errors.map(e => e.$message as string)"
          :error="v$.title.$invalid && v$.title.$dirty"
        ></v-text-field>
        <v-textarea :model-value="modelValue.story"
          @update:model-value="(newValue) => { updateModelValue({ story: newValue }); v$.story.$touch(); }"
          :label="t('memberStory.storyEditor.storyContent')" outlined
          auto-grow
          :error-messages="v$.story.$errors.map(e => e.$message as string)"
          :error="v$.story.$invalid && v$.story.$dirty"
        ></v-textarea>
      </v-col>
    </v-row>

    <FaceMemberSelectDialog :show="showSelectMemberDialog" @update:show="showSelectMemberDialog = $event"
      :selected-face="faceToLabel" @label-face="handleLabelFaceAndCloseDialog" :family-id="props.familyId"
      :show-relation-prompt-field="true" :disable-save-validation="true" />
  </v-container>
</template>

<script setup lang="ts">
import { computed, toRefs, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import useVuelidate from '@vuelidate/core';
import { memberStoryValidationRules } from '@/validations/memberStory.validation';
import type { MemberStoryDto } from '@/types/memberStory';
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar, FaceMemberSelectDialog } from '@/components/face';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useMemberStoryForm } from '@/composables/useMemberStoryForm';

const props = defineProps<{
  modelValue: MemberStoryDto;
  readonly?: boolean;
  memberId?: string | null;
  familyId?: string;
}>();
const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles', 'story-generated']);
const { t } = useI18n();

const vuelidateState = computed(() => ({
  memberId: props.modelValue.memberId,
  rawInput: props.modelValue.rawInput,
  title: props.modelValue.title,
  story: props.modelValue.story,
  photoUrl: props.modelValue.photoUrl,
}));

const v$ = useVuelidate(memberStoryValidationRules, vuelidateState);

watch(() => props.modelValue.photoUrl, () => {
  v$.value.photoUrl.$touch();
});

const updateModelValue = (payload: Partial<MemberStoryDto>) => {
  emit('update:modelValue', { ...props.modelValue, ...payload });
};

const onStoryGenerated = (payload: { story: string | null; title: string | null }) => {
  emit('story-generated', payload);
};

const {
  showSelectMemberDialog,
  faceToLabel,
  aiPerspectiveSuggestions,
  storyStyles,
  generatedStory,
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
  memberId: props.memberId,
  familyId: props.familyId,
  updateModelValue,
  onStoryGenerated,
});

defineExpose({
  isValid: computed(() => !v$.value.$invalid),
  memoryFaceStore: memberStoryStoreFaceRecognition,
  generateStory,
  generatedStory,
  generatedTitle,
  storyEditorValid,
});
</script>

<style scoped>
/* Remove stepper-header styles as stepper is removed */
</style>
