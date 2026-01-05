<template>
  <v-navigation-drawer v-model="internalModelValue" location="right" temporary :width="width" :scrim="scrim">
    <v-toolbar  density="compact">
      <v-btn icon="mdi-close" variant="text" @click="closeDrawer" size="small"></v-btn>
      <v-toolbar-title  v-if="title" class="text-h6 text-uppercase text-center mr-8">{{ title }}</v-toolbar-title>
    </v-toolbar>
    <slot></slot>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

interface BaseCrudDrawerProps {
  modelValue: boolean;
  width?: string | number;
  scrim?: boolean;
  title?: string; // Added title prop
}

const props = withDefaults(defineProps<BaseCrudDrawerProps>(), {
  width: 750,
  scrim: true,
  title: '' // Default empty title
});

const emit = defineEmits(['update:modelValue', 'close']);

const internalModelValue = ref(props.modelValue);

watch(
  () => props.modelValue,
  (newVal) => {
    internalModelValue.value = newVal;
  },
);

watch(internalModelValue, (newVal) => {
  emit('update:modelValue', newVal);
});

const closeDrawer = () => {
  internalModelValue.value = false;
  emit('close');
};
</script>
