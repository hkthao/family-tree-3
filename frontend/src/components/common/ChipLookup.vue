<template>
  <v-chip-group>
    <v-chip
      v-for="item in selectedItems"
      :key="item[valueExpr]"
      size="small"
    >
      <v-avatar v-if="imageExpr && item[imageExpr]" start>
        <v-img :src="item[imageExpr]"></v-img>
      </v-avatar>
      {{ item[displayExpr] }}
    </v-chip>
  </v-chip-group>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';

// Define Props
interface ChipLookupProps {
  dataSource: any[] | any; // Can be an array or a Pinia store
  modelValue: (string | number)[] | undefined; // Array of IDs
  displayExpr?: string;
  valueExpr?: string;
  imageExpr?: string;
}

const props = withDefaults(defineProps<ChipLookupProps>(), {
  displayExpr: 'name',
  valueExpr: 'id',
  imageExpr: undefined,
});

const selectedItems = ref<any[]>([]);

// Check if dataSource is a Pinia store
const isStore = computed(() => {
  return (
    props.dataSource &&
    typeof props.dataSource === 'object' &&
    '_p' in props.dataSource
  );
});

const loadItems = async () => {
  if (!props.modelValue || props.modelValue.length === 0) {
    selectedItems.value = [];
    return;
  }

  if (isStore.value && typeof props.dataSource.getManyItemsByIds === 'function') {
    selectedItems.value = await props.dataSource.getManyItemsByIds(props.modelValue);
  } else if (Array.isArray(props.dataSource)) {
    selectedItems.value = props.dataSource.filter((item) =>
      props.modelValue?.includes(item[props.valueExpr])
    );
  }
};

watch(() => props.modelValue, loadItems, { immediate: true });
</script>