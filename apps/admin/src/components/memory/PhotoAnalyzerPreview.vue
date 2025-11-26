<template>
  <v-card flat>
    <v-card-title>{{ t('memory.photoAnalysis.title') }}</v-card-title>
    <v-card-text v-if="analysisResult">
      <v-row dense>
        <v-col cols="12">
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title class="text-subtitle-1">{{ t('memory.photoAnalysis.summary') }}</v-list-item-title>
              <v-list-item-subtitle>{{ analysisResult.summary }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-col>

        <v-col cols="12" md="6">
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title class="text-subtitle-1">{{ t('memory.photoAnalysis.scene') }}</v-list-item-title>
              <v-list-item-subtitle>{{ analysisResult.scene }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-col>
        <v-col cols="12" md="6">
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title class="text-subtitle-1">{{ t('memory.photoAnalysis.event') }}</v-list-item-title>
              <v-list-item-subtitle>{{ analysisResult.event }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-col>
        <v-col cols="12" md="6">
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title class="text-subtitle-1">{{ t('memory.photoAnalysis.emotion') }}</v-list-item-title>
              <v-list-item-subtitle>{{ analysisResult.emotion }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-col>
        <v-col cols="12" md="6">
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title class="text-subtitle-1">{{ t('memory.photoAnalysis.yearEstimate') }}</v-list-item-title>
              <v-list-item-subtitle>{{ analysisResult.yearEstimate }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-col>
        <v-col cols="12">
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title class="text-subtitle-1">{{ t('memory.photoAnalysis.objects') }}</v-list-item-title>
              <v-list-item-subtitle>{{ analysisResult.objects?.join(', ') || t('common.none') }}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-col>
        <v-col cols="12" v-if="analysisResult.persons && analysisResult.persons.length">
          <v-list-item>
            <v-list-item-content>
              <v-list-item-title class="text-subtitle-1">{{ t('memory.photoAnalysis.persons') }}</v-list-item-title>
              <v-list-item-subtitle>
                <v-chip-group>
                  <v-chip v-for="(person, index) in analysisResult.persons" :key="index" color="primary">
                    {{ person.name || t('memory.photoAnalysis.unknownPerson') }} ({{ ((person.confidence ?? 0) * 100).toFixed(0) }}%)
                  </v-chip>
                </v-chip-group>
              </v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
        </v-col>
      </v-row>
    </v-card-text>
    <v-card-text v-else>
      <v-alert type="info">{{ t('memory.photoAnalysis.noResult') }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="primary" @click="emit('useContext')">{{ t('memory.photoAnalysis.useContext') }}</v-btn>
      <v-btn color="secondary" @click="emit('editContext')">{{ t('memory.photoAnalysis.edit') }}</v-btn>
      <v-btn color="grey" @click="emit('skip')">{{ t('memory.photoAnalysis.skip') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { PhotoAnalysisResultDto } from '@/types/ai'; // Only PhotoAnalysisResultDto needed

interface Props {
  analysisResult: PhotoAnalysisResultDto | null;
}

const { analysisResult } = defineProps<Props>(); // Destructure props
const emit = defineEmits(['useContext', 'editContext', 'skip']);
const { t } = useI18n();
</script>

<style scoped>
/* Scoped styles for PhotoAnalyzerPreview */
</style>
