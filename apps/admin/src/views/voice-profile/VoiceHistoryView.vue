<template>
  <v-card :elevation="0" class="voice-history-view">
    <v-card-title class="headline text-center">{{ t('voiceProfile.history.title') }}</v-card-title>
    <v-overlay :model-value="isLoading" class="align-center justify-center" contained scrim="#E0E0E0">
      <v-progress-circular color="primary" indeterminate size="64"></v-progress-circular>
    </v-overlay>
    <v-card-text>
      <div v-if="isLoading" class="text-center">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p>{{ t('common.loading') }}</p>
      </div>
      <v-alert v-else-if="error" type="error" class="mb-4">{{ error?.message || t('voiceProfile.history.errorLoading')
        }}</v-alert>
      <v-list v-else-if="voiceGenerations.length > 0">
        <v-sheet v-for="(item, index) in voiceGenerations" :key="item.id" class="mb-2 pa-4" rounded elevation="1">
          <div class="d-flex align-center">
            <v-icon class="mr-2">mdi-volume-high</v-icon>
            <div class="w-100">
              <div class="text-subtitle-1">{{ item.text }}</div>
              <audio controls :src="item.audioUrl" class="w-100"></audio>
            </div>
          </div>
          <v-divider v-if="index < voiceGenerations.length - 1" class="my-2"></v-divider>
        </v-sheet>
      </v-list>
      <v-alert v-else type="info" variant="tonal" class="ma-2">{{ t('voiceProfile.history.noHistory') }}</v-alert>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="gray" @click="emit('close')" data-testid="button-close">{{ t('common.close') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQueryClient } from '@tanstack/vue-query';
import { format } from 'date-fns';
import type { VoiceGenerationDto } from '@/types';
import { useVoiceGenerationHistoryQuery } from '@/composables/voice-profile/useVoiceGenerationHistoryQuery';

const props = defineProps<{
  voiceProfileId: string;
}>();

const emit = defineEmits(['close']);

const { t } = useI18n();
const queryClient = useQueryClient();

const voiceGenerations = ref<VoiceGenerationDto[]>([]);

const filters = computed(() => ({
  voiceProfileId: props.voiceProfileId,
}));

const { data, isLoading, error, refetch } = useVoiceGenerationHistoryQuery(filters);

watch(data, (newVal) => {
  if (newVal) {
    voiceGenerations.value = newVal; // Directly assign the array
  }
}, { immediate: true });

const formatDate = (dateString: string) => {
  if (!dateString) return '';
  return format(new Date(dateString), 'dd/MM/yyyy HH:mm:ss');
};

onMounted(() => {
  refetch();
});
</script>

<style scoped>
.voice-history-view {
  min-height: 400px;
  /* Adjust as needed */
}
</style>
