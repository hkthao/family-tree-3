<template>
  <v-card class="mt-5" :elevation="0">
    <v-progress-linear :active="loading" :indeterminate="loading" color="primary" absolute top></v-progress-linear>
    <v-card-title class="pa-0">Nhập thông tin mô tả về thành viên</v-card-title>
    <v-card-subtitle class="text-caption text-wrap pa-0">
      Chức năng này hỗ trợ cho việc thêm mới một thành viên dựa trên mô tả. Nếu thành viên đã tồn tại trong hệ thống,
      bạn nên chỉnh sửa thủ công để đảm bảo độ chính xác cao nhất.
    </v-card-subtitle>
    <v-card-text class="pa-0">
      <v-textarea class="mt-4" v-model="editorContent" :placeholder="t('naturalLanguage.editor.mentionInstruction')"
        clearable maxlength="2000" hide-details></v-textarea>
    </v-card-text>
    <v-card-actions class="pa-0">
      <span class="text-caption">
        {{ t('naturalLanguage.editor.characterCount') }}: {{ editorContent.length }} / 2000
      </span>
      <v-spacer></v-spacer>
      <v-btn color="primary" @click="clearEditor" :loading="loading" :disabled="loading">{{
        t('naturalLanguage.editor.clear') }}</v-btn>
      <v-btn color="primary" @click="emitContent" :loading="loading" :disabled="loading">{{
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

const clearEditor = () => {
  editorContent.value = '';
};
</script>

<style lang="scss">
/* No specific styles needed for v-textarea beyond Vuetify defaults */
/* Removed tiptap-editor specific styles */
</style>