<template>
  <v-navigation-drawer v-model="internalModelValue" location="right" temporary :width="width" :scrim="scrim">
    <v-card class="d-flex flex-column h-full" :elevation="0">
      <v-btn icon="mdi-close" variant="text" @click="closeDrawer" size="small" class="mx-2 mt-2" absolute top right></v-btn>
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
