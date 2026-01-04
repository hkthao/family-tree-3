<template>
  <v-navigation-drawer v-model="internalModelValue" location="right" temporary :width="width" :scrim="scrim">
    <v-btn icon="mdi-close" variant="text" @click="closeDrawer" size="small" class="mx-2 mt-2 btn-close"></v-btn>
    <slot></slot>
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
  width: 750,
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
.btn-close{
  position: absolute;
  z-index: 100;
}
</style>
