<template>
  <v-list-item>
    <template v-slot:prepend>
      <v-avatar size="40" rounded="sm">
        <v-img :src="createBase64ImageSrc(face.thumbnail)" alt="Face"></v-img>
      </v-avatar>
    </template>
    <v-list-item-title>
      {{ face.memberName || t('common.unknown') }}
      <v-chip v-if="face.emotion" class="ml-2" size="x-small">{{ face.emotion }}</v-chip>
    </v-list-item-title>
    <v-list-item-subtitle v-if="face.relationPrompt">
      {{ face.relationPrompt }}
    </v-list-item-subtitle>
    <v-list-item-subtitle v-if="face.memberId">
      <div class="text-caption text-medium-emphasis">{{ face.familyName }}</div>
      <div class="text-caption text-medium-emphasis">
        <span v-if="face.birthYear">{{ face.birthYear }}</span>
        <span v-if="face.birthYear && face.deathYear"> - </span>
        <span v-if="face.deathYear">{{ face.deathYear }}</span>
      </div>
    </v-list-item-subtitle>
  </v-list-item>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { DetectedFace } from '@/types';
import { createBase64ImageSrc } from '@/utils/image.utils';

const { face } = defineProps<{
  face: DetectedFace;
}>();

const { t } = useI18n();
</script>