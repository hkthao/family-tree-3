<template>
  <v-container>
    <!-- Cover Image -->
    <v-row v-if="modelValue.resizedImageUrl">
      <v-col cols="12" class="text-center">
        <v-img :src="modelValue.resizedImageUrl" contain class="mb-4"></v-img>
      </v-col>
    </v-row>

    <!-- Member -->
    <v-row v-if="modelValue.memberId">
      <v-col cols="12">
        <MemberAutocomplete
          :model-value="modelValue.memberId"
          :label="t('memberStory.form.memberIdLabel')"
          disabled
        ></MemberAutocomplete>
      </v-col>
    </v-row>

    <!-- Raw Input -->
    <v-row v-if="modelValue.rawInput">
      <v-col cols="12">
        <v-textarea
          :label="t('memberStory.create.rawInputPlaceholder')"
          :model-value="modelValue.rawInput"
          readonly
          auto-grow
          rows="2"
          variant="outlined"
          density="compact"
          class="mb-2"
        ></v-textarea>
      </v-col>
    </v-row>

    <!-- Story Style -->
    <v-row v-if="modelValue.storyStyle">
      <v-col cols="12">
        <v-text-field
          :label="t('memberStory.create.storyStyle.question')"
          :model-value="getStoryStyleText(modelValue.storyStyle)"
          readonly
          variant="outlined"
          density="compact"
          class="mb-2"
        ></v-text-field>
      </v-col>
    </v-row>

    <!-- Perspective -->
    <v-row v-if="modelValue.perspective">
      <v-col cols="12">
        <v-text-field
          :label="t('memberStory.create.perspective.question')"
          :model-value="getPerspectiveText(modelValue.perspective)"
          readonly
          variant="outlined"
          density="compact"
          class="mb-2"
        ></v-text-field>
      </v-col>
    </v-row>

    <!-- Title -->
    <v-row>
      <v-col cols="12">
        <v-text-field
          :label="t('memberStory.storyEditor.title')"
          :model-value="modelValue.title"
          @update:model-value="(newValue) => { updateModelValue({ title: newValue }); }"
          variant="outlined"
          density="compact"
          class="mb-2"
          :rules="[rules.title.required]"
        ></v-text-field>
      </v-col>
    </v-row>

    <!-- Story -->
    <v-row>
      <v-col cols="12">
        <v-textarea
          :label="t('memberStory.storyEditor.storyContent')"
          :model-value="modelValue.story"
          @update:model-value="(newValue) => { updateModelValue({ story: newValue }); }"
          auto-grow
          variant="outlined"
          density="compact"
          class="mb-2"
          :rules="[rules.story.required]"
        ></v-textarea>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberStoryDto } from '@/types/memberStory';
import { MemberStoryPerspective, MemberStoryStyle } from '@/types/enums';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';


const props = defineProps<{
  modelValue: MemberStoryDto;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

const rules = {
  title: {
    required: (value: string | null) => !!value || t('common.validations.required'),
  },
  story: {
    required: (value: string | null) => !!value || t('common.validations.required'),
  },
};

const updateModelValue = (payload: Partial<MemberStoryDto>) => {
  emit('update:modelValue', { ...props.modelValue, ...payload });
};

// Helper to get display text for story style
const getStoryStyleText = (style: MemberStoryStyle): string => {
  switch (style) {
    case MemberStoryStyle.Nostalgic: return t('memberStory.style.nostalgic');
    case MemberStoryStyle.Warm: return t('memberStory.style.warm');
    case MemberStoryStyle.Formal: return t('memberStory.style.formal');
    case MemberStoryStyle.Folk: return t('memberStory.style.folk');
    default: return '';
  }
};

// Helper to get display text for perspective
const getPerspectiveText = (perspective: MemberStoryPerspective): string => {
  switch (perspective) {
    case MemberStoryPerspective.FirstPerson: return t('memberStory.create.perspective.firstPerson');
    case MemberStoryPerspective.ThirdPerson: return t('memberStory.create.perspective.thirdPerson');
    case MemberStoryPerspective.FamilyMember: return t('memberStory.create.perspective.familyMember');
    case MemberStoryPerspective.NeutralPersonal: return t('memberStory.create.perspective.neutralPersonal');
    case MemberStoryPerspective.FullyNeutral: return t('memberStory.create.perspective.fullyNeutral');
    default: return '';
  }
};

const titleValid = computed(() => !!props.modelValue.title);
const storyValid = computed(() => !!props.modelValue.story);

defineExpose({
  isValid: computed(() => titleValid.value && storyValid.value),
});
</script>

<style scoped>
/* Add any specific styles for this component if necessary */
</style>
