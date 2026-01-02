<template>
  <v-data-table-server v-model:items-per-page="itemsPerPage" v-model:page="page" v-model:sort-by="sortBy" :items="items"
    :items-length="totalItems" :loading="loading" :headers="headers" class="elevation-0"
    data-testid="image-restoration-job-list">
    <template #top>
      <ListToolbar :title="t('imageRestorationJob.list.title')"
        :create-button-tooltip="t('imageRestorationJob.list.add')"
        create-button-test-id="button-create-image-restoration-job" @create="emit('create')">
      </ListToolbar>
    </template>
    <template #item.status="{ item }">
      <v-chip :color="getStatusColor(item.status)" small>
        {{ item.status ? t(`imageRestorationJob.status.${String(item.status).toLowerCase()}`) : '' }}
      </v-chip>
    </template>
    <template #item.originalImageUrl="{ item }">
      <v-img :src="item.originalImageUrl" height="50" width="50" cover class="my-1"></v-img>
    </template>
    <template #item.restoredImageUrl="{ item }">
      <v-img v-if="item.restoredImageUrl" :src="item.restoredImageUrl" height="50" width="50" cover
        class="my-1"></v-img>
      <span v-else>{{ t('common.na') }}</span>
    </template>
    <template #item.created="{ item }">
      {{ formatDate(item.created) }}
    </template>
    <template #item.actions="{ item }">
      <v-tooltip :text="t('common.delete')" location="top">
        <template v-slot:activator="{ props }">
          <v-icon v-bind="props" small @click="emit('delete', item.id)" data-testid="button-delete">
            mdi-delete
          </v-icon>
        </template>
      </v-tooltip>
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { type ImageRestorationJobDto } from '@/types/imageRestorationJob';
import { formatDate } from '@/utils/format.utils';
import ListToolbar from '@/components/common/ListToolbar.vue'; // Import ListToolbar

interface ImageRestorationJobListProps {
  items: ImageRestorationJobDto[];
  totalItems: number;
  loading: boolean;
  familyId: string;
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
  canPerformActions?: boolean;
}

defineProps<ImageRestorationJobListProps>();
const emit = defineEmits(['update:options', 'create', 'view', 'edit', 'delete']);

const { t } = useI18n();

const itemsPerPage = ref(10);
const page = ref(1);
const sortBy = ref<any[]>([]);

// Define the Header type with explicit align literal types
interface DataTableHeader {
  title: string;
  key: string;
  sortable?: boolean;
  align?: 'start' | 'end' | 'center';
  width?: string | number;
}

const headers = computed<DataTableHeader[]>(() => [
  { title: t('imageRestorationJob.list.headers.originalImageUrl'), key: 'originalImageUrl', sortable: false },
  { title: t('imageRestorationJob.list.headers.status'), key: 'status', sortable: true },
  { title: t('imageRestorationJob.list.headers.restoredImageUrl'), key: 'restoredImageUrl', sortable: false },
  { title: t('imageRestorationJob.list.headers.created'), key: 'created', sortable: true },
  {
    title: t('common.actions'), key: 'actions', sortable: false,
    align: 'center',
  },
]);

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Processing':
      return 'blue';
    case 'Completed':
      return 'green';
    case 'Failed':
      return 'red';
    default:
      return 'grey';
  }
};

watch([itemsPerPage, page, sortBy], () => {
  emit('update:options', {
    page: page.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value,
  });
});
</script>