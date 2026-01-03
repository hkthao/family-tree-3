<template>
  <div>
    <ListToolbar :title="t('voiceProfile.list.title')" :create-button-tooltip="t('voiceProfile.list.add')"
      create-button-test-id="button-create-voice-profile" @create="emit('create')" :search-query="searchQuery"
      @update:search="emit('update:search', $event)">
      <template #append-buttons>
        <v-btn v-if="canPerformActions" class="ml-2" color="info" data-testid="button-import-voice-profile"
          :loading="isImporting" @click="emit('on-import-click')">
          {{ t('common.import') }}
        </v-btn>
        <v-btn v-if="canPerformActions" class="ml-2" color="info" data-testid="button-export-voice-profile"
          :loading="isExporting" @click="emit('on-export')">
          {{ t('common.export') }}
        </v-btn>
      </template>
    </ListToolbar>

    <v-data-table-server v-model:items-per-page="itemsPerPage" v-model:page="page" v-model:sort-by="sortBy"
      :headers="headers" :items="items" :items-length="totalItems" :loading="loading" item-value="id"
      class="elevation-1">
      <template #item.label="{ item }">
        <span @click.stop="emit('view', item.id)" class="text-primary font-weight-bold" style="cursor: pointer;">
          {{ item.label }}
        </span>
      </template>

      <template #item.audioPlay="{ item }">
        <audio class="mt-2" v-if="item.audioUrl" :src="item.audioUrl" controls></audio>
      </template>

      <template v-slot:item.actions="{ item }">
        <v-tooltip v-if="allowEdit" :text="t('common.edit')" location="top">
          <template v-slot:activator="{ props: tooltipProps }">
            <v-btn icon v-bind="tooltipProps" @click="emit('edit', item.id)" variant="text" data-testid="button-edit"
              size="small">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
        <v-tooltip v-if="allowDelete" :text="t('common.delete')" location="top">
          <template v-slot:activator="{ props: tooltipProps }">
            <v-btn icon v-bind="tooltipProps" @click="emit('delete', item.id)" variant="text"
              data-testid="button-delete" size="small">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
      </template>
      <template #item.status="{ item }">
        <v-chip :color="getStatusColor(item.status)" small>
          {{ item.status !== undefined ? t(`voiceProfile.status.${VoiceProfileStatus[item.status].toLowerCase()}`) :
            '' }}
        </v-chip>
      </template>
      <template #item.created="{ item }">
        {{ formatDate(item.created) }}
      </template>
    </v-data-table-server>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { VoiceProfileStatus, type VoiceProfile } from '@/types';
import ListToolbar from '@/components/common/ListToolbar.vue';
import { format } from 'date-fns';

interface VoiceProfileListProps {
  items: VoiceProfile[];
  totalItems: number;
  loading: boolean;

  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
  isExporting?: boolean;
  isImporting?: boolean;
  canPerformActions?: boolean;
  searchQuery?: string;
}

withDefaults(defineProps<VoiceProfileListProps>(), {
  allowAdd: true,
  allowEdit: true,
  allowDelete: true,
  isExporting: false,
  isImporting: false,
  canPerformActions: true,
});

const emit = defineEmits([
  'update:options',
  'create',
  'view',
  'edit',
  'delete',
  'on-export',
  'on-import-click',
  'update:search',
]);

const { t } = useI18n();

const formatDate = (dateString: string) => {
  if (!dateString) return '';
  return format(new Date(dateString), 'dd/MM/yyyy');
};

// Define the Header type with explicit align literal types
interface DataTableHeader {
  title: string;
  key: string;
  sortable?: boolean;
  align?: 'start' | 'end' | 'center';
  width?: string | number;
}

const itemsPerPage = ref(10);
const page = ref(1);
const sortBy = ref<any[]>([]);

const headers = computed<DataTableHeader[]>(() => [
  { title: t('voiceProfile.list.headers.audio'), key: 'audioPlay', sortable: false, width: 80 },
  { title: t('voiceProfile.list.headers.name'), key: 'label' },
  { title: t('voiceProfile.list.headers.status'), key: 'status', width: 120 },
  { title: t('voiceProfile.list.headers.created'), key: 'created', width: 120 },
  { title: t('voiceProfile.list.headers.actions'), key: 'actions', sortable: false, width: 120 },
]);

const getStatusColor = (status: VoiceProfileStatus) => {
  switch (status) {
    case VoiceProfileStatus.Active:
      return 'green';
    case VoiceProfileStatus.Archived:
      return 'grey';
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
