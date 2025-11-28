<template>
  <v-container fluid class="pa-0">
    <h4>{{ t('memory.create.storyStyle.question') }}</h4>
    <v-chip-group :model-value="modelValue" @update:model-value="(newValue) => emit('update:modelValue', newValue)"
      color="primary" mandatory column :disabled="readonly">
      <v-chip v-for="style in storyStyles" :key="style.value" :value="style.value" filter variant="tonal">
        {{ style.text }}
      </v-chip>
    </v-chip-group>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps<{
  modelValue: string | null | undefined;
  readonly?: boolean;
}>();
const emit = defineEmits(['update:modelValue']);
const { t } = useI18n();

// Placeholder story styles. These should ideally come from a constants file or API.
const storyStyles = ref([
  { value: 'narrative', text: t('memory.create.storyStyle.narrative') },
  { value: 'descriptive', text: t('memory.create.storyStyle.descriptive') },
  { value: 'reflective', text: t('memory.create.storyStyle.reflective') },
  { value: 'journalistic', text: t('memory.create.storyStyle.journalistic') },
  { value: 'poetic', text: t('memory.create.storyStyle.poetic') },
]);
</script>

<style scoped></style>