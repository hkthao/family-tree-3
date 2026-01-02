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
        {{ item.status !== undefined ? t(`imageRestorationJob.status.${RestorationStatus[item.status].toLowerCase()}`) :
          '' }}
      </v-chip>
    </template>
    <template #item.originalImageUrl="{ item }">
      <div class="d-flex justify-center w-100">
        <ImageEnlarger :src="item.originalImageUrl" :alt="t('imageRestorationJob.list.headers.originalImageUrl')"
          thumbnail-height="80" thumbnail-width="50">
        </ImageEnlarger>
      </div>
    </template>
    <template #item.restoredImageUrl="{ item }">
      <div class="d-flex justify-center w-100">
        <ImageEnlarger v-if="item.restoredImageUrl" :src="item.restoredImageUrl"
          :alt="t('imageRestorationJob.list.headers.restoredImageUrl')" thumbnail-height="50" thumbnail-width="50">
        </ImageEnlarger>
        <span v-else>{{ t('common.na') }}</span>
      </div>
    </template>
    <template #item.created="{ item }">
      {{ formatDate(item.created) }}
    </template>
    <template #item.actions="{ item }">
      <v-tooltip :text="t('common.viewDetails')" location="top">
        <template v-slot:activator="{ props }">
          <v-btn icon v-bind="props" @click="emit('view', item.id)" variant="text" data-testid="button-view-details" size="small">
            <v-icon>mdi-eye</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
      <v-tooltip :text="t('common.delete')" location="top">
        <template v-slot:activator="{ props }">
          <v-btn icon v-bind="props" @click="emit('delete', item.id)" variant="text" data-testid="button-delete" size="small">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { type ImageRestorationJobDto, RestorationStatus } from '@/types';
import { formatDate } from '@/utils/format.utils';
import ListToolbar from '@/components/common/ListToolbar.vue'; // Import ListToolbar
import ImageEnlarger from '@/components/common/ImageEnlarger.vue';

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
const emit = defineEmits(['update:options', 'create', 'view', 'edit', 'delete', 'viewDetails']);

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
  {
    title: t('imageRestorationJob.list.headers.originalImageUrl'), key: 'originalImageUrl', sortable: false,
    align: 'center',
  },
  {
    title: t('imageRestorationJob.list.headers.status'), key: 'status', sortable: true,
    align: 'center',
  },
  {
    title: t('imageRestorationJob.list.headers.restoredImageUrl'), key: 'restoredImageUrl', sortable: false,
    align: 'center',
  },
  {
    title: t('imageRestorationJob.list.headers.created'), key: 'created', sortable: true,
    align: 'center',
  },
  {
    title: t('common.actions'), key: 'actions', sortable: false,
    align: 'center',
  },
]);

const getStatusColor = (status: RestorationStatus) => {
  switch (status) {
    case RestorationStatus.Processing:
      return 'blue';
    case RestorationStatus.Completed:
      return 'green';
    case RestorationStatus.Failed:
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

<style scoped></style>
