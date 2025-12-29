<template>
  <v-alert v-if="isPrivate" type="info" prominent class="mb-4">
    {{ t('common.privacyAlert') }}
  </v-alert>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

defineProps<{
  isPrivate?: boolean;
}>();

const { t } = useI18n();
</script>

<style scoped></style>
