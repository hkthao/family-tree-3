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
    <v-row>
      <v-col cols="12">
        <h4 class="mb-2">{{ t('memberStory.create.storyStyle.question') }}</h4>
        <v-chip-group
          :model-value="modelValue.storyStyle"
          @update:model-value="(newValue) => updateModelValue({ storyStyle: newValue })"
          color="primary"
          mandatory
          column
        >
          <v-chip
            v-for="style in storyStyles"
            :key="style.value"
            :value="style.value"
            filter
            variant="tonal"
          >
            {{ style.text }}
          </v-chip>
        </v-chip-group>
      </v-col>
    </v-row>

    <!-- Perspective -->
    <v-row>
      <v-col cols="12">
        <h4 class="mb-2">{{ t('memberStory.create.perspective.question') }}</h4>
        <v-chip-group
          :model-value="modelValue.perspective"
          @update:model-value="(newValue) => updateModelValue({ perspective: newValue })"
          color="primary"
          mandatory
          column
        >
          <v-chip
            v-for="perspective in aiPerspectiveSuggestions"
            :key="perspective.value"
            :value="perspective.value"
            filter
            variant="tonal"
          >
            {{ perspective.text }}
          </v-chip>
        </v-chip-group>
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
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberStoryDto } from '@/types/memberStory';
import { MemberStoryPerspective, MemberStoryStyle } from '@/types/enums';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';


const props = defineProps<{
  modelValue: MemberStoryDto;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

const aiPerspectiveSuggestions = computed(() => ([
  { value: MemberStoryPerspective.FirstPerson, text: t('memberStory.create.perspective.firstPerson') },
  { value: MemberStoryPerspective.NeutralPersonal, text: t('memberStory.create.perspective.neutralPersonal') },
  { value: MemberStoryPerspective.FullyNeutral, text: t('memberStory.create.perspective.fullyNeutral') },
]));

const storyStyles = computed(() => ([
  { value: MemberStoryStyle.Nostalgic, text: t('memberStory.style.nostalgic') },
  { value: MemberStoryStyle.Warm, text: t('memberStory.style.warm') },
  { value: MemberStoryStyle.Formal, text: t('memberStory.style.formal') },
  { value: MemberStoryStyle.Folk, text: t('memberStory.style.folk') },
]));

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

const titleValid = computed(() => !!props.modelValue.title);
const storyValid = computed(() => !!props.modelValue.story);

defineExpose({
  isValid: computed(() => titleValid.value && storyValid.value),
});
</script>
