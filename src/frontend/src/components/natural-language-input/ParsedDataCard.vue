<template>
  <v-card>
    <v-card-title>{{ t('naturalLanguage.confirmation.title') }}</v-card-title>
    <v-card-subtitle>{{ t('naturalLanguage.confirmation.entityType') }}: {{ entityType }}</v-card-subtitle>
    <v-card-text>
      <v-list dense>
        <v-list-item v-for="(value, key) in parsedData" :key="key">
          <v-list-item-content>
            <v-list-item-title>{{ key }}</v-list-item-title>
            <v-list-item-subtitle>{{ value }}</v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="cancel">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="save">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

interface ParsedDataCardProps {
  parsedData: object;
  entityType: string;
}

const props = defineProps<ParsedDataCardProps>();
const emit = defineEmits(['save', 'cancel']);

const { t } = useI18n();

const save = () => {
  emit('save');
};

const cancel = () => {
  emit('cancel');
};
</script>
