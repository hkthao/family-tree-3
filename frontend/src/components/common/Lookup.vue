<template>
  <div>
    <v-select
      :model-value="props.modelValue"
      :items="selectedItem ? [selectedItem] : []"
      :item-title="displayExpr"
      :item-value="valueExpr"
      :label="computedLabel"
      :loading="loading"
      :clearable="true"
      :rules="rules"
      :disabled="props.disabled"
      variant="outlined"
      density="compact"
      hide-dropdown-icon
      @click="openDialog"
    >
    </v-select>

    <v-dialog v-model="dialog" max-width="800px">
      <v-card>
        <v-card-title>
          <span class="text-h5">{{ label }}</span>
        </v-card-title>
        <v-card-text>
          <v-text-field
            ref="searchInput"
            v-model="searchTerm"
            :label="t('common.search')"
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
            hover
            @update:options="loadItems"
            @click:row="selectItem"
          ></v-data-table-server>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue-darken-1" variant="text" @click="closeDialog">
            {{ t('common.close') }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';

// Define Props
interface LookupProps {
  dataSource: any[] | any; // Can be an array or a Pinia store
  modelValue: string | number | null | undefined; // v-model:value
  displayExpr?: string;
  valueExpr?: string;
  label?: string;
  clearable?: boolean;
  rules?: any[];
  subtitleExpr?: string; // New prop for subtitle
  additionalFilters?: any; // New prop for additional filters
  disabled?: boolean; // New prop for disabling the component
}

const { t } = useI18n(); // Moved here

const props = withDefaults(defineProps<LookupProps>(), {
  displayExpr: 'name',
  valueExpr: 'id',
  label: undefined, // Remove default label here
  clearable: false,
  rules: () => [],
  subtitleExpr: undefined, // Default value for subtitleExpr
  disabled: false, // Default value for disabled
});

// Define Emits
const emit = defineEmits(['update:modelValue']);

const computedLabel = computed(() => props.label || t('common.selectItem'));

// Internal state
const dialog = ref(false);
const loading = ref(false);
const items = ref<any[]>([]);
const totalItems = ref(0);
const searchTerm = ref('');
const searchInput = ref<HTMLInputElement | null>(null); // Ref for search input
const selectedItem = ref<any>(null);

// Headers for the data table
const headers = computed(() => {
  const cols: any[] = [{ title: t('common.name'), value: props.displayExpr }];
  if (props.subtitleExpr) {
    cols.push({ title: t('common.subtitle'), value: props.subtitleExpr });
  }
  return cols;
});

// Check if dataSource is a Pinia store
const isStore = computed(() => {
  // A more robust check for Pinia store instance
  return (
    props.dataSource &&
    typeof props.dataSource === 'object' &&
    '_p' in props.dataSource
  );
});

// Preload selected item label
watch(
  () => props.modelValue,
  async (newValue) => {
    if (newValue && !selectedItem.value) {
      if (isStore.value && typeof props.dataSource.getItemById === 'function') {
        loading.value = true;
        selectedItem.value = await props.dataSource.getItemById(newValue);
        loading.value = false;
      } else if (Array.isArray(props.dataSource)) {
        selectedItem.value = props.dataSource.find(
          (item) => item[props.valueExpr] === newValue,
        );
      }
    }
  },
  { immediate: true },
);

// Watch for changes in additionalFilters and reload items
watch(
  () => props.additionalFilters,
  () => {
    loadItems({ page: 1, itemsPerPage: 10, sortBy: [] });
  },
);

// Load items for the dialog table
const loadItems = async ({
  page,
  itemsPerPage,
  sortBy,
}: {
  page: number;
  itemsPerPage: number;
  sortBy: any[];
}) => {
  if (!isStore.value || typeof props.dataSource.searchLookup !== 'function')
    return;

  loading.value = true;
  try {
    const filter = {
      searchQuery: searchTerm.value,
      ...props.additionalFilters,
    }; // Merge with additional filters
    await props.dataSource.searchLookup(filter, page, itemsPerPage);
    items.value = props.dataSource.items;
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
const openDialog = async () => {
  dialog.value = true;
  await nextTick(); // Ensure dialog is rendered before focusing
  searchInput.value?.focus();
  loadItems({ page: 1, itemsPerPage: 10, sortBy: [] });
};

const closeDialog = () => {
  dialog.value = false;
};

// Item selection
const selectItem = (event: Event, { item }: { item: any }) => {
  selectedItem.value = item;
  emit('update:modelValue', item[props.valueExpr]);
  closeDialog();
};
</script>
