<template>
  <v-dialog v-model="dialog" max-width="400">
    <v-card>
      <v-card-title class="headline">{{ t('family.tree.printOptions') }}</v-card-title>
      <v-card-text>
        <v-select
          v-model="selectedPageSize"
          :items="pageSizes"
          :label="t('family.tree.printOptions.pageSize')"
          item-title="text"
          item-value="value"
          variant="outlined"
          class="mb-4"
        ></v-select>
        <v-select
          v-model="selectedDirection"
          :items="directions"
          :label="t('family.tree.printOptions.direction')"
          item-title="text"
          item-value="value"
          variant="outlined"
          class="mb-4"
        ></v-select>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey-darken-1" variant="text" @click="cancel">{{ t('common.cancel') }}</v-btn>
        <v-btn color="primary" variant="elevated" @click="generatePdf">{{ t('family.tree.printOptions.generatePdf') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps<{
  modelValue: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'generate', options: { pageSize: string; direction: string }): void;
}>();

const { t } = useI18n();

const dialog = ref(props.modelValue);
const selectedPageSize = ref('A0'); // Default value
const selectedDirection = ref('LR'); // Default value

const pageSizes = [
  { text: 'A0', value: 'A0' },
  { text: 'A1', value: 'A1' },
  { text: 'A2', value: 'A2' },
  { text: 'A3', value: 'A3' },
  { text: 'A4', value: 'A4' },
  // Add more as needed
];

const directions = [
  { text: t('family.tree.printOptions.direction.lr'), value: 'LR' }, // Left to Right
  { text: t('family.tree.printOptions.direction.tb'), value: 'TB' }, // Top to Bottom
];

watch(() => props.modelValue, (newVal) => {
  dialog.value = newVal;
});

watch(dialog, (newVal) => {
  emit('update:modelValue', newVal);
});

const cancel = () => {
  dialog.value = false;
};

const generatePdf = () => {
  emit('generate', { pageSize: selectedPageSize.value, direction: selectedDirection.value });
  dialog.value = false;
};
</script>
