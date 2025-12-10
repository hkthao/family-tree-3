<template>
  <v-card class="mt-5" elevation="2">
    <v-card-title class="d-flex align-center">
      {{ t('familyMedia.list.title') }}
      <v-spacer></v-spacer>
      <v-btn
        color="primary"
        prepend-icon="mdi-plus"
        @click="emit('create')"
      >
        {{ t('familyMedia.list.createButton') }}
      </v-btn>
    </v-card-title>
    <v-data-table-server
      :headers="headers"
      :items="props.items"
      :items-length="props.totalItems"
      :loading="props.loading"
      :items-per-page="10"
      @update:options="handleOptionsUpdate"
      item-value="id"
      class="elevation-1"
    >
      <template v-slot:item.thumbnailPath="{ item }">
        <v-img v-if="item.thumbnailPath" :src="item.thumbnailPath" max-height="50" max-width="50" class="my-2"></v-img>
        <v-icon v-else>mdi-file-image-outline</v-icon>
      </template>
      <template v-slot:item.fileName="{ item }">
        <a :href="item.filePath" target="_blank">{{ item.fileName }}</a>
      </template>
      <template v-slot:item.mediaType="{ item }">
        {{ t(`common.mediaType.${MediaType[item.mediaType]}`) }}
      </template>
      <template v-slot:item.fileSize="{ item }">
        {{ formatBytes(item.fileSize) }}
      </template>
      <template v-slot:item.created="{ item }">
        {{ formatDate(item.created) }}
      </template>
      <template v-slot:item.actions="{ item }">
        <v-icon
          small
          class="me-2"
          @click="emit('view', item.id)"
        >
          mdi-eye
        </v-icon>
        <v-icon
          small
          class="me-2"
          @click="emit('edit', item.id)"
        >
          mdi-pencil
        </v-icon>
        <v-icon
          small
          @click="emit('delete', item.id)"
        >
          mdi-delete
        </v-icon>
      </template>
      <template v-slot:no-data>
        <v-alert :value="true" color="info" icon="mdi-information">
          {{ t('familyMedia.list.noData') }}
        </v-alert>
      </template>
    </v-data-table-server>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyMedia, ListOptions } from '@/types';
import { MediaType } from '@/types/enums';
import { formatDate, formatBytes } from '@/utils/format.utils'; // Assuming these utilities exist

interface FamilyMediaListProps {
  items: FamilyMedia[];
  totalItems: number;
  loading: boolean;
}

const props = defineProps<FamilyMediaListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create']);
const { t } = useI18n();

const headers = ref([
  { title: '', key: 'thumbnailPath', sortable: false },
  { title: t('familyMedia.list.headers.fileName'), key: 'fileName' },
  { title: t('familyMedia.list.headers.mediaType'), key: 'mediaType' },
  { title: t('familyMedia.list.headers.fileSize'), key: 'fileSize' },
  { title: t('familyMedia.list.headers.description'), key: 'description' },
  { title: t('familyMedia.list.headers.uploadedBy'), key: 'uploadedBy' },
  { title: t('familyMedia.list.headers.created'), key: 'created' },
  { title: t('familyMedia.list.headers.actions'), key: 'actions', sortable: false },
]);

const handleOptionsUpdate = (options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) => {
  emit('update:options', options);
};
</script>
