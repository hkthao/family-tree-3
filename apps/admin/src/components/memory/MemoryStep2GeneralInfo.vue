<template>
  <v-form ref="formStep2">
    <v-row>
      <v-col cols="12">
        <MemberAutocomplete class="mt-2" v-model="internalMemory.memberId" :label="t('member.form.member')"
          :rules="readonly ? [] : [(v: string) => !!v || t('common.validations.required')]"
          :readonly="readonly || !!memberId" required></MemberAutocomplete>
      </v-col>

      <v-col cols="12">
        <AiSuggestionsForm v-model="internalMemory" :readonly="readonly" />
      </v-col>

      <v-col cols="12">
        <v-text-field v-model="internalMemory.title" :label="t('memory.storyEditor.title')"
          :rules="readonly ? [] : [(v: string) => !!v || t('common.validations.required')]" :readonly="readonly"
          required></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="internalMemory.rawInput" :label="t('memory.create.rawInputPlaceholder')"
          :readonly="readonly"></v-textarea>
      </v-col>
      <v-col cols="12" v-if="internalMemory.story">
        <v-textarea v-model="internalMemory.story" :label="t('memory.storyEditor.storyContent')" readonly></v-textarea>
      </v-col>
      <v-col cols="12">
        <v-combobox v-model="internalMemory.tags" :label="t('memory.storyEditor.tags')" chips multiple clearable
          :readonly="readonly"></v-combobox>
      </v-col>
      <v-col cols="12">
        <v-combobox v-model="internalMemory.keywords" :label="t('memory.storyEditor.keywords')" chips multiple clearable
          :readonly="readonly"></v-combobox>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberAutocomplete } from '@/components/common';
import AiSuggestionsForm from './AiSuggestionsForm.vue';
import type { MemoryDto } from '@/types/memory';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string | null;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();
const formStep2 = ref<HTMLFormElement | null>(null);

const internalMemory = computed<MemoryDto>({
  get: () => {
    const model = props.modelValue;
    return {
      ...model,
      rawInput: model.rawInput ?? undefined,
      story: model.story ?? undefined,
      eventSuggestion: model.eventSuggestion ?? undefined,
      customEventDescription: model.customEventDescription ?? undefined,
      emotionContextTags: model.emotionContextTags ?? [],
      customEmotionContext: model.customEmotionContext ?? undefined,
      faces: model.faces ?? [],
    } as MemoryDto;
  },
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

watch(() => props.memberId, (newMemberId) => {
  if (newMemberId && !internalMemory.value.memberId) {
    internalMemory.value.memberId = newMemberId;
  }
}, { immediate: true });

const validate = async () => {
  return formStep2.value ? (await formStep2.value.validate()).valid : false;
};

defineExpose({
  validate,
});
</script>