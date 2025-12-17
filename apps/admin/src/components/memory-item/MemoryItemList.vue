<template>
  <v-data-table-server
    :headers="headers"
    :items="props.items"
    :items-length="props.totalItems"
    :loading="props.loading"
    item-value="id"
    @update:options="emit('update:options', $event)"
    data-testid="memory-item-list"
    class="elevation-1"
  >
    <template #top>
      <ListToolbar
        :title="t('memoryItem.list.title')"
        :create-button-tooltip="t('memoryItem.list.add')"
        create-button-test-id="create-memory-item-button"
        @create="emit('create')"
        :hide-create-button="!props.allowAdd"
        :hide-search="true"
      />
    </template>
    <template #item.title="{ item }">
      <span class="text-primary cursor-pointer text-decoration-underline" @click="emit('view', item.id)">
        {{ item.title }}
      </span>
    </template>
    <template #item.happenedAt="{ item }">
      {{ formatDate(item.happenedAt) }}
    </template>
    <template #item.emotionalTag="{ item }">
      {{ t(`memoryItem.emotionalTag.${EmotionalTag[item.emotionalTag].toLowerCase()}`) }}
    </template>
    <template #item.actions="{ item }">
      <div class="d-flex ga-2" v-if="props.allowEdit || props.allowDelete">
        <v-btn icon size="small" variant="text" color="primary" @click="emit('edit', item.id)" v-if="props.allowEdit">
          <v-icon>mdi-pencil</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.edit') }}</v-tooltip>
        </v-btn>
        <v-btn icon size="small" variant="text" color="error" @click="emit('delete', item.id)" v-if="props.allowDelete">
          <v-icon>mdi-delete</v-icon>
          <v-tooltip activator="parent" location="top">{{ t('common.delete') }}</v-tooltip>
        </v-btn>
      </div>
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { MemoryItem } from '@/types';
import { EmotionalTag } from '@/types/enums';
import { computed } from 'vue';
import ListToolbar from '@/components/common/ListToolbar.vue';
import type { DataTableHeader } from 'vuetify';
import { formatDate } from '@/utils/format.utils';

interface MemoryItemListProps {
  items: MemoryItem[];
  totalItems: number;
  loading: boolean;
  familyId: string;
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
}

const props = defineProps<MemoryItemListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create']);
const { t } = useI18n();

const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    { title: t('memoryItem.list.headers.title'), key: 'title' },
    { title: t('memoryItem.list.headers.happenedAt'), key: 'happenedAt' },
    { title: t('memoryItem.list.headers.emotionalTag'), key: 'emotionalTag' },
  ];

  if (props.allowEdit || props.allowDelete) {
    baseHeaders.push({ title: t('common.actions'), key: 'actions', sortable: false });
  }

  return baseHeaders;
});
</script>
