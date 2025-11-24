<template>
  <v-navigation-drawer v-model="internalModelValue" location="right" temporary :width="width" :scrim="scrim">
    <v-card class="d-flex flex-column h-full" :elevation="0">
      <v-card-title class="d-flex align-center">
        <v-icon v-if="icon" :icon="icon" class="mr-2"></v-icon>
        <span class="text-h6">{{ title }}</span>
        <v-spacer></v-spacer>
        <v-btn icon="mdi-close" variant="text" @click="closeDrawer" size="small"></v-btn>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="flex-grow-1 overflow-y-auto">
        <slot></slot>
      </v-card-text>
    </v-card>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

interface BaseCrudDrawerProps {
  modelValue: boolean;
  title: string;
  icon?: string;
  width?: string | number;
  scrim?: boolean;
}

const props = withDefaults(defineProps<BaseCrudDrawerProps>(), {
  width: 650,
  scrim: true,
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

<style scoped>
/* Add any specific styles for the drawer here */
</style>
