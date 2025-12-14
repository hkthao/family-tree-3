<template>
  <v-card class="mt-5" :elevation="0">
    <v-progress-linear :active="loading" :indeterminate="loading" color="primary" absolute top></v-progress-linear>
<v-card-title>{{ t('nlEditor.title') }}</v-card-title>
    <v-card-subtitle class="text-caption text-wrap">
      {{ t('nlEditor.subtitle') }}
    </v-card-subtitle>
    <v-card-text class="pa-0">
      <v-textarea class="mt-4" v-model="editorContent" :placeholder="t('nlEditor.placeholder')"
        clearable maxlength="2000" hide-details></v-textarea>
    </v-card-text>
    <v-card-actions class="pa-0">
      <span class="text-caption">
        {{ t('naturalLanguage.editor.characterCount') }}: {{ editorContent.length }} / 2000
      </span>
      <v-spacer></v-spacer>
      <v-btn color="primary" @click="emitContent" :loading="loading" :disabled="loading || !editorContent">{{
        t('naturalLanguage.editor.parseButton') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const editorContent = ref(''); // Use a ref for v-textarea content

defineProps<{
  loading: boolean;
}>();

const emit = defineEmits(['parse-content']);

const emitContent = () => {
  emit('parse-content', editorContent.value);
};
</script>

<style lang="scss">
/* No specific styles needed for v-textarea beyond Vuetify defaults */
/* Removed tiptap-editor specific styles */
</style>