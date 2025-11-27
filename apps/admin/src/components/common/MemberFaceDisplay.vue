<template>
  <v-list-item>
    <template v-slot:prepend>
      <v-avatar size="40" rounded="sm" class="mr-2">
        <v-img :src="createBase64ImageSrc(face.thumbnail)" alt="Face"></v-img>
      </v-avatar>
    </template>
    <v-list-item-title>
      {{ t('member.form.member') }}: {{ face.memberName || t('common.unknown') }}
    </v-list-item-title>
    <v-list-item-subtitle v-if="face.relationPrompt">
      {{ t('memory.create.aiCharacterSuggestion.relationPrompt') }}: {{ face.relationPrompt }}
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