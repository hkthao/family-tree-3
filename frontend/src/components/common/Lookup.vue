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
      <template #selection="{ item }">
        <div class="d-flex align-center">
          <v-avatar
            v-if="imageExpr && item.raw[imageExpr]"
            :image="item.raw[imageExpr]"
            size="32"
            class="mr-2"
          ></v-avatar>
          <span>{{ item.title }}</span>
        </div>
      </template>
    </v-select>

    <v-dialog v-model="dialog" max-width="600px" scrollable>
      <v-card>
        <v-card-title class="d-flex align-center">
          <span class="text-h5">{{ label }}</span>
          <v-spacer></v-spacer>
          <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>

        <v-card-text>
          <v-text-field
            ref="searchInput"
            v-model="searchTerm"
            :label="t('common.search')"
            prepend-inner-icon="mdi-magnify"
            variant="solo-filled"
            flat
            clearable
            autofocus
            hide-details
            @input="searchItems"
          ></v-text-field>
        </v-card-text>

        <v-divider></v-divider>

        <v-list v-if="!loading && items.length > 0" lines="two">
          <v-list-item
            v-for="item in items"
            :key="item[valueExpr]"
            :title="item[displayExpr]"
            :subtitle="subtitleExpr ? item[subtitleExpr] : undefined"
            @click="selectItem(item)"
          >
            <template #prepend>
              <v-avatar
                v-if="imageExpr && item[imageExpr]"
                :image="item[imageExpr]"
                class="mr-4"
              ></v-avatar>
            </template>
          </v-list-item>
        </v-list>

        <div
          v-if="loading"
          class="d-flex justify-center align-center pa-4"
          style="min-height: 200px"
        >
          <v-progress-circular indeterminate color="primary"></v-progress-circular>
        </div>

        <div
          v-if="!loading && items.length === 0"
          class="text-center pa-4 text-grey"
        >
          {{ t('common.noData') }}
        </div>
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
  imageExpr?: string; // New prop for image
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
  imageExpr: undefined, // Default value for imageExpr
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
const searchTerm = ref('');
const searchInput = ref<HTMLInputElement | null>(null); // Ref for search input
const selectedItem = ref<any>(null);

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
const selectItem = (item: any) => {
  selectedItem.value = item;
  emit('update:modelValue', item[props.valueExpr]);
  closeDialog();
};
</script>
