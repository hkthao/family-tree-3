<template>
  <v-card class="pa-4">
    <v-card-title class="text-h5">{{ $t('chunkTable.title') }}</v-card-title>
    <v-card-text>
      <v-data-table :headers="headers" :items="chunks" :items-per-page="10" item-value="id" show-select
        v-model="selectedChunks">
        <template v-slot:item.contentPreview="{ item }">
          {{ item.content.substring(0, 100) + (item.content.length > 100 ? '...' : '') }}
        </template>
        <template v-slot:item.metadata.fileName="{ item }">
          {{ item.metadata.fileName }}
        </template>
        <template v-slot:item.metadata.page="{ item }">
          {{ item.metadata.page }}
        </template>
        <template v-slot:item.approved="{ item }">
          <v-checkbox v-model="item.approved" @update:model-value="onApprovalChange(item.id, item.approved)"
            hide-details></v-checkbox>
        </template>
      </v-data-table>
    </v-card-text>
    <v-card-actions v-if="chunks.length > 0">
      <v-spacer></v-spacer>
      <v-btn color="error" :disabled="selectedChunks.length === 0" @click="rejectSelected">
        {{ $t('chunkTable.rejectSelectedButton') }}
      </v-btn>
      <v-btn color="success" :disabled="selectedChunks.length === 0" @click="approveSelected">
        {{ $t('chunkTable.approveSelectedButton') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VDataTable } from 'vuetify/components';
import type { TextChunk } from '@/types';
import type { DataTableHeader } from 'vuetify';


interface Props {
  chunks: TextChunk[];
}

const props = defineProps<Props>();
const emit = defineEmits(['chunk-approval-changed', 'approve-selected', 'reject-selected']);
const { t } = useI18n();

const headers: DataTableHeader[] = [
  { title: t('chunkTable.headers.id'), key: 'id' },
  { title: t('chunkTable.headers.contentPreview'), key: 'contentPreview' },
  { title: t('chunkTable.headers.fileName'), key: 'metadata.fileName' },
  { title: t('chunkTable.headers.page'), key: 'metadata.page', align: 'center' },
  { title: t('chunkTable.headers.approved'), key: 'approved', align: 'center' },
];

const selectedChunks = ref<string[]>([]);

const onApprovalChange = (chunkId: string, approved: boolean) => {
  emit('chunk-approval-changed', chunkId, approved);
};

const approveSelected = () => {
  emit('approve-selected', selectedChunks.value);
  selectedChunks.value = []; // Clear selection after action
};

const rejectSelected = () => {
  emit('reject-selected', selectedChunks.value);
  selectedChunks.value = []; // Clear selection after action
};

// Watch for changes in props.chunks and update selectedChunks if necessary
watch(() => props.chunks, (newChunks) => {
  // If a chunk is removed from the list, ensure it's not in selectedChunks
  selectedChunks.value = selectedChunks.value.filter(id => newChunks.some(chunk => chunk.id === id));
});
</script>
