<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="d-flex align-center">
            {{ t('voiceProfile.list.title') }}
            <v-spacer></v-spacer>
            <v-btn
              v-if="allowAdd"
              color="primary"
              data-testid="button-create-voice-profile"
              @click="emit('create')"
            >
              {{ t('voiceProfile.list.add') }}
            </v-btn>
            <v-btn
              v-if="canPerformActions"
              class="ml-2"
              color="info"
              data-testid="button-import-voice-profile"
              :loading="isImporting"
              @click="emit('on-import-click')"
            >
              {{ t('common.import') }}
            </v-btn>
            <v-btn
              v-if="canPerformActions"
              class="ml-2"
              color="info"
              data-testid="button-export-voice-profile"
              :loading="isExporting"
              @click="emit('on-export')"
            >
              {{ t('common.export') }}
            </v-btn>
          </v-card-title>

          <v-card-text>
            <v-data-table-server
              :headers="headers"
              :items="items"
              :items-length="totalItems"
              :loading="loading"
              item-value="id"
              @update:options="handleUpdateOptions"
              class="elevation-1"
            >
              <template v-slot:item.actions="{ item }">
                <v-icon
                  small
                  class="mr-2"
                  @click="emit('view', item.id)"
                  data-testid="button-view"
                >
                  mdi-eye
                </v-icon>
                <v-icon
                  v-if="allowEdit"
                  small
                  class="mr-2"
                  @click="emit('edit', item.id)"
                  data-testid="button-edit"
                >
                  mdi-pencil
                </v-icon>
                <v-icon
                  v-if="allowDelete"
                  small
                  @click="emit('delete', item.id)"
                  data-testid="button-delete"
                >
                  mdi-delete
                </v-icon>
              </template>
            </v-data-table-server>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VoiceProfile } from '@/types';

interface VoiceProfileListProps {
  items: VoiceProfile[];
  totalItems: number;
  loading: boolean;
  memberId: string; // Assuming memberId is still relevant for context
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
  isExporting?: boolean;
  isImporting?: boolean;
  canPerformActions?: boolean;
  onExport?: () => void;
  onImportClick?: () => void;
}

const props = withDefaults(defineProps<VoiceProfileListProps>(), {
  allowAdd: true,
  allowEdit: true,
  allowDelete: true,
  isExporting: false,
  isImporting: false,
  canPerformActions: true,
  onExport: () => {},
  onImportClick: () => {},
});

const emit = defineEmits([
  'update:options',
  'create',
  'view',
  'edit',
  'delete',
  'on-export',
  'on-import-click',
]);

const { t } = useI18n();

const headers = ref([
  { title: t('voiceProfile.list.headers.name'), key: 'label' },
  { title: t('voiceProfile.list.headers.status'), key: 'status' },
  { title: t('voiceProfile.list.headers.created'), key: 'created' },
  { title: t('voiceProfile.list.headers.actions'), key: 'actions', sortable: false },
]);

const handleUpdateOptions = (options: any) => {
  emit('update:options', options);
};
</script>

<style scoped></style>
