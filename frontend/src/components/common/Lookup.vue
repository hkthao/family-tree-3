<template>
  <div>
    <v-select
      :model-value="props.modelValue"
      :items="selectedItem ? [selectedItem] : []"
      :item-title="displayExpr"
      :item-value="valueExpr"
      :label="label"
      :loading="loading"
      :clearable="clearable"
      :rules="rules"
      readonly
      variant="outlined"
      density="compact"
      hide-dropdown-icon
      @click:append-inner="openDialog"
    >
      <template #append-inner>
        <v-icon @click.stop="openDialog">mdi-magnify</v-icon>
      </template>
    </v-select>

    <v-dialog v-model="dialog" max-width="800px">
      <v-card>
        <v-card-title>
          <span class="text-h5">{{ label }}</span>
        </v-card-title>
        <v-card-text>
          <v-text-field
            v-model="searchTerm"
            label="Search"
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            density="compact"
            clearable
            @input="searchItems"
          ></v-text-field>
          <v-data-table-server
            :headers="headers"
            :items="items"
            :items-length="totalItems"
            :loading="loading"
            :search="searchTerm"
            @update:options="loadItems"
            @click:row="selectItem"
          ></v-data-table-server>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue-darken-1" variant="text" @click="closeDialog">
            Close
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';

// Define Props
interface LookupProps {
  dataSource: any[] | any; // Can be an array or a Pinia store
  modelValue: string | number | null | undefined; // v-model:value
  displayExpr?: string;
  valueExpr?: string;
  label?: string;
  clearable?: boolean;
  rules?: any[];
}

const props = withDefaults(defineProps<LookupProps>(), {
  displayExpr: 'name',
  valueExpr: 'id',
  label: 'Select an item',
  clearable: false,
  rules: () => [],
});

// Define Emits
const emit = defineEmits(['update:modelValue']);

// Internal state
const dialog = ref(false);
const loading = ref(false);
const items = ref<any[]>([]);
const totalItems = ref(0);
const searchTerm = ref('');
const selectedItem = ref<any>(null);

// Headers for the data table
const headers = computed(() => [
  { title: props.displayExpr, value: props.displayExpr },
  // Add more headers if needed
]);

// Check if dataSource is a Pinia store
const isStore = computed(() => {
  // A more robust check for Pinia store instance
  return props.dataSource && typeof props.dataSource === 'object' && '_p' in props.dataSource;
});

// Preload selected item label
watch(() => props.modelValue, async (newValue) => {
  if (newValue && !selectedItem.value) {
    if (isStore.value && typeof props.dataSource.fetchFamilyById === 'function') {
      loading.value = true;
      selectedItem.value = await props.dataSource.fetchFamilyById(newValue);
      loading.value = false;
    } else if (Array.isArray(props.dataSource)) {
      selectedItem.value = props.dataSource.find(item => item[props.valueExpr] === newValue);
    }
  }
}, { immediate: true });

// Load items for the dialog table
const loadItems = async ({ page, itemsPerPage, sortBy }: { page: number; itemsPerPage: number; sortBy: any[] }) => {
  if (!isStore.value || typeof props.dataSource.searchLookup !== 'function') return;

  loading.value = true;
  try {
    await props.dataSource.searchLookup(searchTerm.value, page, itemsPerPage);
    items.value = props.dataSource.families;
    totalItems.value = props.dataSource.totalItems;
  } catch (error) {
    console.error('Error loading items:', error);
  } finally {
    loading.value = false;
  }
};


// Search items
const searchItems = () => {
  loadItems({ page: 1, itemsPerPage: 10, sortBy: [] }); // Reset pagination on search
};

// Dialog control
const openDialog = () => {
  dialog.value = true;
  loadItems({ page: 1, itemsPerPage: 10, sortBy: [] });
};

const closeDialog = () => {
  dialog.value = false;
};

// Item selection
const selectItem = (event: Event, { item }: { item: { raw: any } }) => {
  selectedItem.value = item.raw;
  emit('update:modelValue', item.raw[props.valueExpr]);
  closeDialog();
};

</script>