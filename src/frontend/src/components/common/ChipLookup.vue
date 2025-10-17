<template>
  <div class="chip-lookup-group">
    <v-chip v-for="item in selectedItems" :key="item[valueExpr]" size="small">
      <v-avatar v-if="imageExpr && item[imageExpr]" start>
        <v-img :src="item[imageExpr]"></v-img>
      </v-avatar>
      {{ item[displayExpr] }}
    </v-chip>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';

// Define Props
interface ChipLookupProps {
  dataSource: any[] | any; // Can be an array or a Pinia store
  modelValue: (string | number | (string | number)[]) | undefined; // Single ID or Array of IDs
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
  let idsToFetch: (string | number)[] = [];

  if (props.modelValue) {
    if (Array.isArray(props.modelValue)) {
      idsToFetch = props.modelValue;
    } else {
      idsToFetch = [props.modelValue];
    }
  }

  if (idsToFetch.length === 0) {
    selectedItems.value = [];
    return;
  }

  if (
    isStore.value &&
    typeof props.dataSource.getByIds === 'function'
  ) {
    selectedItems.value = await props.dataSource.getByIds(idsToFetch);
  } else if (Array.isArray(props.dataSource)) {
    selectedItems.value = props.dataSource.filter((item) =>
      idsToFetch.includes(item[props.valueExpr]),
    );
  }
};

watch(() => props.modelValue, loadItems, { immediate: true });
</script>
<style scoped>
.chip-lookup-group {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  padding: 4px 0px;
}
</style>
