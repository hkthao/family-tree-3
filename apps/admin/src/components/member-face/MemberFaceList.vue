<template>
  <v-card :elevation="0">
    <v-card-title class="d-flex align-center">
      <div class="text-h6 text-uppercase">{{ t('memberFace.list.title') }}</div>
      <v-spacer></v-spacer>
      <v-btn
        color="primary"
        @click="emit('create')"
        data-testid="create-member-face-button"
      >
        <v-icon left>mdi-plus</v-icon>
        {{ t('common.create') }}
      </v-btn>
    </v-card-title>
    <v-card-text>
      <v-data-table-server
        v-model:items-per-page="itemsPerPage"
        v-model:page="page"
        v-model:sort-by="sortBy"
        :headers="headers"
        :items="items"
        :items-length="totalItems"
        :loading="loading"
        class="elevation-0"
        item-value="id"
        @update:options="handleUpdateOptions"
      >
        <template v-slot:item.confidence="{ item }">
          {{ item.confidence?.toFixed(2) }}
        </template>
        <template v-slot:item.emotionConfidence="{ item }">
          {{ item.emotionConfidence?.toFixed(2) }}
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn
            icon
            flat
            size="small"
            @click="emit('view', item)"
            data-testid="view-member-face-button"
          >
            <v-icon>mdi-eye</v-icon>
          </v-btn>
          <v-btn
            icon
            flat
            size="small"
            @click="emit('edit', item)"
            data-testid="edit-member-face-button"
          >
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
          <v-btn
            icon
            flat
            size="small"
            @click="emit('delete', item)"
            color="error"
            data-testid="delete-member-face-button"
          >
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
        <template #bottom></template>
      </v-data-table-server>
    </v-card-text>
    <v-card-actions class="d-flex justify-end">
      <v-pagination
        v-model="page"
        :length="Math.ceil(totalItems / itemsPerPage)"
        :total-visible="5"
        rounded="circle"
      ></v-pagination>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberFace } from '@/types';

interface MemberFaceListProps {
  items: MemberFace[];
  totalItems: number;
  loading: boolean;
}

const props = defineProps<MemberFaceListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create']);

const { t } = useI18n();

const page = ref(1);
const itemsPerPage = ref(10);
const sortBy = ref<any[]>([]); // { key: string, order: 'asc' | 'desc' }[]

const headers = computed(() => [
  { title: t('memberFace.list.headers.faceId'), key: 'faceId' },
  { title: t('memberFace.list.headers.memberName'), key: 'memberName' },
  { title: t('memberFace.list.headers.confidence'), key: 'confidence' },
  { title: t('memberFace.list.headers.emotion'), key: 'emotion' },
  { title: t('memberFace.list.headers.emotionConfidence'), key: 'emotionConfidence' },
  { title: t('memberFace.list.headers.actions'), key: 'actions', sortable: false },
]);

// Watch for changes in pagination/sorting options and emit to parent
watch([page, itemsPerPage, sortBy], () => {
  emit('update:options', {
    page: page.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value,
  });
}, { deep: true });

// Handle direct update from v-data-table-server
const handleUpdateOptions = (options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[]; }) => {
  page.value = options.page;
  itemsPerPage.value = options.itemsPerPage;
  sortBy.value = options.sortBy;
};
</script>