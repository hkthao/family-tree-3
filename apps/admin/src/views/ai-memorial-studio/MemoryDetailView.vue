<template>
  <v-card-title class="text-center">
    <span class="text-h6">{{ memory?.title || t('memory.detail.titleDefault') }}</span>
  </v-card-title>
  <v-card-text>
    <MemoryForm
      v-if="memory"
      v-model="memory"
      :readonly="true"
      :member-id="memory.memberId"
    />
    <v-alert v-else-if="memoryStore.error" type="error" dismissible class="mb-4">
      {{ memoryStore.error }}
    </v-alert>
    <v-alert v-else type="info">{{ t('memory.detail.loading') }}</v-alert>
  </v-card-text>
  <v-card-actions>
    <v-spacer></v-spacer>
    <v-btn color="blue-darken-1" variant="text" @click="emit('close')">
      {{ t('common.close') }}
    </v-btn>
    <v-btn color="blue-darken-1" variant="text" @click="emit('edit-item', memory?.id)">
      {{ t('common.edit') }}
    </v-btn>
  </v-card-actions>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemoryDto } from '@/types/memory'; // Make sure this import is correct
import MemoryForm from '@/components/memory/MemoryForm.vue';

const props = defineProps<{
  memoryId: string;
}>();

const emit = defineEmits(['close', 'edit-item']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const { showSnackbar } = useGlobalSnackbar();

const memory = ref<MemoryDto | null>(null);

const fetchMemory = async (id: string) => {
  memory.value = await memoryStore.getById(id) || null;
  if (!memory.value) {
    showSnackbar(t('memory.detail.notFound'), 'error');
  }
};

onMounted(() => {
  if (props.memoryId) {
    fetchMemory(props.memoryId);
  }
});

watch(() => props.memoryId, (newId) => {
  if (newId) {
    fetchMemory(newId);
  }
});
</script>
