<template>
  <v-card class="mb-4 mt-4">
    <v-card-text>
      <v-data-table-server
        :items-per-page="itemsPerPage"
        @update:itemsPerPage="$emit('update:itemsPerPage', $event)"
        :headers="headers"
        :items="families"
        :items-length="totalFamilies"
        :loading="loading"
        item-value="id"
        @update:options="$emit('update:options', $event)"
        elevation="1"
      >
        <template #top>
          <v-toolbar flat>
            <v-toolbar-title>{{ t('family.management.title') }}</v-toolbar-title>
            <v-spacer></v-spacer>
            <v-btn color="primary" icon @click="$emit('create')">
              <v-icon>mdi-plus</v-icon>
            </v-btn>
          </v-toolbar>
        </template>
        <!-- Avatar column -->
        <template #item.avatarUrl="{ item }">
          <div class="d-flex justify-center">
            <v-avatar size="36" class="my-2">
              <v-img v-if="item.avatarUrl" :src="item.avatarUrl" :alt="item.name" />
              <v-icon v-else>mdi-account-group</v-icon>
            </v-avatar>
          </div>
        </template>

                  <!-- name column -->
                  <template #item.name="{ item }">
                    <div class="text-left">
                      <div
                        class="text-primary text-none"
                        style="cursor: pointer;"
                        @click="$emit('view', item)"
                      >
                        {{ item.name }}
                      </div>
                    </div>
                  </template>
        <!-- visibility column -->
        <template #item.visibility="{ item }">
          <v-chip
            :color="item.visibility === 'Public' ? 'success' : 'error'"
            label
            size="small"
            class="text-capitalize"
          >
            {{ $t(`family.management.visibility.${item.visibility.toLowerCase()}`) }}
          </v-chip>
        </template>

        <!-- Actions column -->
        <template #item.actions="{ item }">
          <v-btn icon size="small" variant="text" @click="$emit('edit', item)">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
          <v-btn icon size="small" variant="text" @click="$emit('delete', item)">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>

        <!-- Loading state -->
        <template #loading>
          <v-skeleton-loader type="table-row@5" />
        </template>
      </v-data-table-server>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family } from '@/types/family';
import type { DataTableHeader } from 'vuetify';

const props = defineProps<{
  families: Family[];
  totalFamilies: number;
  loading: boolean;
  itemsPerPage: number;
}>();

const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'update:itemsPerPage', 'create']);

const { t } = useI18n();

const headers = computed<DataTableHeader[]>(() => [
  { title: t('family.management.headers.avatar'), key: 'avatarUrl', sortable: false, width: '120px', align: 'center' },
  { title: t('family.management.headers.name'), key: 'name', width: 'auto', align: 'start' },
  { title: t('family.management.headers.visibility'), key: 'visibility', width: '120px', align: 'center' },
  { title: t('family.management.headers.actions'), key: 'actions', sortable: false, width: '120px', align: 'center' },
]);
</script>