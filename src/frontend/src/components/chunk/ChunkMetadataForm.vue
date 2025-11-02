<template>
  <v-row>
    <v-col cols="12" md="6">
      <v-text-field v-model="internalFileId" :label="$t('chunkUpload.fileIdLabel')"
        :rules="[v => !!v || $t('chunkUpload.fileIdRequired')]" required ></v-text-field>
    </v-col>
    <v-col cols="12" md="6">
      <v-text-field v-model="internalFamilyId" :label="$t('chunkUpload.familyIdLabel')"
        ></v-text-field>
    </v-col>
    <v-col cols="12" md="6">
      <v-text-field
        v-model="internalCategory"
        :label="$t('chunkUpload.categoryLabel')"
        :rules="[v => !!v || $t('chunkUpload.categoryRequired')]"
        
        clearable
      >
        <v-menu
          activator="parent"
          :close-on-content-click="true"
          v-model="showSuggestions"
        >
          <v-list v-if="filteredCategories.length > 0">
            <v-list-item
              v-for="(category, index) in filteredCategories"
              :key="index"
              @click="selectCategory(category)"
            >
              <v-list-item-title>{{ category }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </v-text-field>
    </v-col>
    <v-col cols="12" md="6">
      <v-text-field v-model="internalCreatedBy" :label="$t('chunkUpload.createdByLabel')" :readonly="true"
         :rules="[v => !!v || $t('chunkUpload.createdByRequired')]"
        required></v-text-field>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';

const props = defineProps({
  fileId: { type: String, default: '' },
  familyId: { type: String, default: '' },
  category: { type: String, default: '' },
  createdBy: { type: String, default: '' },
});

const emit = defineEmits(['update:fileId', 'update:familyId', 'update:category', 'update:createdBy']);

const internalFileId = ref(props.fileId);
const internalFamilyId = ref(props.familyId);
const internalCategory = ref(props.category);
const internalCreatedBy = ref(props.createdBy);

const showSuggestions = ref(false);

// Dummy list of categories for demonstration
const availableCategories = ref([
  'Thông tin cá nhân',
  'Lịch sử gia đình',
  'Sự kiện quan trọng',
  'Tài liệu',
  'Hình ảnh',
  'Video',
  'Âm thanh',
  'Gia phả',
  'Tiểu sử',
  'Hồ sơ pháp lý',
  'Thư từ',
  'Nghiên cứu',
  'Kiến thức cho AI',
]);

const filteredCategories = computed(() => {
  if (!internalCategory.value) {
    return availableCategories.value;
  }
  const searchText = internalCategory.value.toLowerCase();
  return availableCategories.value.filter(category =>
    category.toLowerCase().includes(searchText)
  );
});

const selectCategory = (category: string) => {
  internalCategory.value = category;
  showSuggestions.value = false;
};

watch(() => props.fileId, (newVal) => {
  internalFileId.value = newVal;
});
watch(() => props.familyId, (newVal) => {
  internalFamilyId.value = newVal;
});
watch(() => props.category, (newVal) => {
  internalCategory.value = newVal;
});
watch(() => props.createdBy, (newVal) => {
  internalCreatedBy.value = newVal;
});

watch(internalFileId, (newVal) => {
  emit('update:fileId', newVal);
});
watch(internalFamilyId, (newVal) => {
  emit('update:familyId', newVal);
});
watch(internalCategory, (newVal) => {
  emit('update:category', newVal);
  showSuggestions.value = !!newVal && filteredCategories.value.length > 0;
});
watch(internalCreatedBy, (newVal) => {
  emit('update:createdBy', newVal);
});

const validate = () => {
  return !!internalFileId.value && !!internalCategory.value && !!internalCreatedBy.value;
};

defineExpose({ validate });
</script>
