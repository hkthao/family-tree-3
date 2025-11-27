<template>
  <div>
    <h4 class="mt-4">{{ t('memory.create.storyStyle.question') }}</h4>
    <v-chip-group
      :model-value="modelValue"
      @update:model-value="(newValue) => emit('update:modelValue', newValue)"
      color="primary"
      mandatory
      column
      :disabled="readonly"
    >
      <v-chip v-for="style in storyStyles" :key="style" :value="style" filter variant="tonal">
        {{ t(`memory.style.${style}`) }}
      </v-chip>
    </v-chip-group>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { modelValue, readonly } = defineProps<{
  modelValue: string | undefined;
  readonly?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

const storyStyles = ['nostalgic', 'warm', 'formal', 'folk'];
</script>