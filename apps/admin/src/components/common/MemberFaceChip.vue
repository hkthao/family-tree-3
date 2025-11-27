<template>
  <v-chip
    variant="outlined"
    color="primary"
    filter
    :value="value"
    class="chip-member-face"
  >
    <v-avatar left>
      <v-img :src="createBase64ImageSrc(face.thumbnail)" alt="Face"></v-img>
    </v-avatar>
    <span class="ml-2">{{ face.memberName || t('common.unknown') }}</span>
    <v-chip  v-if="face.emotion" class="chip-face-emotion">{{ face.emotion }}</v-chip>
  </v-chip>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { DetectedFace } from '@/types';
import { createBase64ImageSrc } from '@/utils/image.utils';

const { face, value } = defineProps<{
  face: DetectedFace;
  value: string; // NEW PROP
}>();

const { t } = useI18n();
</script>

<style scoped>
.chip-face-emotion{
  margin-left: 10px;
  margin-right: -12px;
}
</style>