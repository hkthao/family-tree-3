<template>
  <div data-testid="image-restoration-job-list-view">
    <ImageRestorationJobList
      :items="imageRestorationJobs"
      :total-items="currentTotalItems"
      :loading="isLoadingImageRestorationJobs"
      :family-id="props.familyId"
      @update:options="handleListOptionsUpdate"
      @create="openAddDrawer()"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      :allow-add="true"
      :allow-edit="true"
      :allow-delete="true"
      :can-perform-actions="true"
    />

    <!-- Add Image Restoration Job Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleImageRestorationJobClosed">
      <ImageRestorationJobAddView v-if="addDrawer" :family-id="props.familyId" @close="handleImageRestorationJobClosed"
        @saved="handleImageRestorationJobSaved" />
    </BaseCrudDrawer>

    <!-- Edit Image Restoration Job Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleImageRestorationJobClosed">
      <ImageRestorationJobEditView v-if="selectedItemId && editDrawer" :family-id="props.familyId"
        :image-restoration-job-id="selectedItemId" @close="handleImageRestorationJobClosed"
        @saved="handleImageRestorationJobSaved" />
    </BaseCrudDrawer>

    <!-- Detail Image Restoration Job Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleImageRestorationJobClosed">
      <ImageRestorationJobDetailView v-if="selectedItemId && detailDrawer" :family-id="props.familyId"
        :image-restoration-job-id="selectedItemId" @close="handleImageRestorationJobClosed" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useCrudDrawer, useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { useImageRestorationJobDataManagement, useImageRestorationJobsQuery, useDeleteImageRestorationJobMutation } from '@/composables';
import { useQueryClient } from '@tanstack/vue-query';
import type { ImageRestorationJobDto } from '@/types/imageRestorationJob';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import ImageRestorationJobList from '@/components/image-restoration-job/ImageRestorationJobList.vue'; // Will create this
import ImageRestorationJobAddView from './ImageRestorationJobAddView.vue';
import ImageRestorationJobEditView from './ImageRestorationJobEditView.vue';
import ImageRestorationJobDetailView from './ImageRestorationJobDetailView.vue';

interface ImageRestorationJobListViewProps {
  familyId: string;
}

const props = defineProps<ImageRestorationJobListViewProps>();
const { t } = useI18n();
const queryClient = useQueryClient();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const {
  state: { paginationOptions, filters }, // queryParams is no longer needed directly here for useImageRestorationJobsQuery
  actions: { setPage, setItemsPerPage, setSortBy },
} = useImageRestorationJobDataManagement(computed(() => props.familyId));

const {
  state: { imageRestorationJobs: queryImageRestorationJobs, isLoading: isLoadingImageRestorationJobs },
} = useImageRestorationJobsQuery(
  computed(() => props.familyId),
  paginationOptions, // Pass paginationOptions as ListOptions
  filters // Pass filters as FilterOptions
);

const imageRestorationJobs = ref<ImageRestorationJobDto[]>(queryImageRestorationJobs.value || []);
const currentTotalItems = ref(0); // Total items will be managed by the backend query directly, or passed via an envelope. For now, hardcode or calculate from queryImageRestorationJobs.

watch(queryImageRestorationJobs, (newData) => {
  imageRestorationJobs.value = newData || [];
  // Assuming totalItems comes from an envelope or can be derived
  currentTotalItems.value = newData?.length || 0;
});


const { mutate: deleteImageRestorationJob } = useDeleteImageRestorationJobMutation();

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  setPage(options.page);
  setItemsPerPage(options.itemsPerPage);
  setSortBy(options.sortBy as { key: string; order: 'asc' | 'desc' }[]);
};

const confirmDelete = async (id: string) => {
  const jobToDelete = imageRestorationJobs.value.find(job => job.jobId === id);
  if (!jobToDelete) {
    showSnackbar(t('imageRestorationJob.messages.notFound'), 'error');
    return;
  }

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('imageRestorationJob.list.confirmDelete', { jobId: jobToDelete.jobId }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    handleDeleteConfirm(jobToDelete.jobId);
  }
};

const handleDeleteConfirm = (id: string) => {
  deleteImageRestorationJob({ jobId: id, familyId: props.familyId }, {
    onSuccess: () => {
      showSnackbar(t('imageRestorationJob.messages.deleteSuccess'), 'success');
      queryClient.invalidateQueries({ queryKey: ['image-restoration-jobs'] });
    },
    onError: (error) => {
      showSnackbar(error.message || t('imageRestorationJob.messages.deleteError'), 'error');
    },
  });
};

const handleImageRestorationJobSaved = () => {
  closeAllDrawers();
  queryClient.invalidateQueries({ queryKey: ['image-restoration-jobs'] });
};

const handleImageRestorationJobClosed = () => {
  closeAllDrawers();
};

</script>

<style scoped></style>