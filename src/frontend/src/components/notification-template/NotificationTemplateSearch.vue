<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ $t('notificationTemplate.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded">
        <v-icon>{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="searchQuery"
                :label="$t('notificationTemplate.search.searchLabel')"
                clearable
                prepend-inner-icon="mdi-magnify"
                hide-details
              />
            </v-col>
            <v-col cols="12" md="4">
              <v-select
                v-model="selectedEventType"
                :items="notificationTypes"
                item-title="title"
                item-value="value"
                :label="$t('notificationTemplate.form.eventType')"
                clearable
                hide-details
              ></v-select>
            </v-col>
            <v-col cols="12" md="4">
              <v-select
                v-model="selectedChannel"
                :items="notificationChannels"
                item-title="title"
                item-value="value"
                :label="$t('notificationTemplate.form.channel')"
                clearable
                hide-details
              ></v-select>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="4">
              <v-select
                v-model="selectedFormat"
                :items="templateFormats"
                item-title="title"
                item-value="value"
                :label="$t('notificationTemplate.form.format')"
                clearable
                hide-details
              ></v-select>
            </v-col>
            <v-col cols="12" md="4">
              <v-select
                v-model="selectedLanguage"
                :items="languageOptions"
                item-title="title"
                item-value="value"
                :label="$t('notificationTemplate.form.languageCode')"
                clearable
                hide-details
              ></v-select>
            </v-col>
            <v-col cols="12" md="4"></v-col> <!-- Empty column to maintain 3-column layout -->
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters">{{ $t('common.apply') }}</v-btn>
          <v-btn @click="resetFilters">{{ $t('common.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { NotificationChannel, NotificationType, TemplateFormat } from '@/types';

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed

const searchQuery = ref('');
const selectedEventType = ref<NotificationType | null>(null);
const selectedChannel = ref<NotificationChannel | null>(null);
const selectedFormat = ref<TemplateFormat | null>(null);
const selectedLanguage = ref<string | null>(null);

const notificationTypes = Object.keys(NotificationType)
  .filter(key => isNaN(Number(key)))
  .map(key => ({
    title: t(`notificationType.${key}`),
    value: NotificationType[key as keyof typeof NotificationType],
  }));

const notificationChannels = Object.keys(NotificationChannel)
  .filter(key => isNaN(Number(key)))
  .map(key => ({
    title: t(`notificationChannel.${key}`),
    value: NotificationChannel[key as keyof typeof NotificationChannel],
  }));

const templateFormats = Object.keys(TemplateFormat)
  .filter(key => isNaN(Number(key)) && TemplateFormat[key as keyof typeof TemplateFormat] !== TemplateFormat.Markdown)
  .map(key => ({
    title: t(`templateFormat.${key}`),
    value: TemplateFormat[key as keyof typeof TemplateFormat],
  }));

const languageOptions = computed(() => [
  { title: t('common.language.en'), value: 'en' },
  { title: t('common.language.vi'), value: 'vi' },
]);

const applyFilters = () => {
  emit('update:filters', {
    search: searchQuery.value,
    eventType: selectedEventType.value,
    channel: selectedChannel.value,
    format: selectedFormat.value,
    languageCode: selectedLanguage.value,
  });
};

const resetFilters = () => {
  searchQuery.value = '';
  selectedEventType.value = null;
  selectedChannel.value = null;
  selectedFormat.value = null;
  selectedLanguage.value = null;
  applyFilters();
};

// Remove automatic filter application on searchQuery change
// watch(searchQuery, () => {
//   applyFilters();
// });
</script>
