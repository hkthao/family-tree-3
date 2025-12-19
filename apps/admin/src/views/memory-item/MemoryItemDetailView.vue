<template>
  <v-card :elevation="0" data-testid="memory-item-detail-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('memoryItem.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert type="error" :text="error?.message || t('memoryItem.detail.errorLoading')"></v-alert>
      </div>
      <div v-else-if="memoryItem">
        <MemoryItemForm :initial-memory-item-data="memoryItem" :family-id="props.familyId" :read-only="true" />
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <!-- Potentially an edit button here, but for now, just close -->
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import MemoryItemForm from '@/components/memory-item/MemoryItemForm.vue';
import { useMemoryItemDetail } from '@/composables';

const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
  memoryItemId: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close']);

const { t } = useI18n();

const { memoryItem, isLoading, error, closeView } = useMemoryItemDetail({
  familyId: props.familyId,
  memoryItemId: props.memoryItemId,
  onClose: () => {
    emit('close');
  },
});
</script>

<style scoped></style>