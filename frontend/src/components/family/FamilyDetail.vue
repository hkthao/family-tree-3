<template>
  <v-card>
    <v-card-title class="d-flex align-center">
      <v-btn v-if="!readOnly" icon @click="$emit('close')" variant="text">
        <v-icon>mdi-arrow-left</v-icon>
      </v-btn>
      <span class="ml-2 text-h5 text-uppercase">{{ title }}</span>
    </v-card-title>
    <v-card-text v-if="initialFamilyData">
      <v-row>
        <v-col cols="12" class="d-flex justify-center">
          <v-avatar size="100" v-if="initialFamilyData.avatarUrl">
            <v-img :src="initialFamilyData.avatarUrl" :alt="initialFamilyData.name"></v-img>
          </v-avatar>
          <v-avatar size="100" color="primary" v-else>
            <span class="text-h5">{{ initialFamilyData.name.charAt(0) }}</span>
          </v-avatar>
        </v-col>
        <v-col cols="12">
          <v-list-item>
            <v-list-item-title class="font-weight-bold">{{ $t('family.detail.name') }}</v-list-item-title>
            <v-list-item-subtitle>{{ initialFamilyData.name }}</v-list-item-subtitle>
          </v-list-item>
        </v-col>
        <v-col cols="12">
          <v-list-item>
            <v-list-item-title class="font-weight-bold">{{ $t('family.detail.description') }}</v-list-item-title>
            <v-list-item-subtitle>{{ initialFamilyData.description || $t('common.na') }}</v-list-item-subtitle>
          </v-list-item>
        </v-col>
        <v-col cols="12">
          <v-list-item>
            <v-list-item-title class="font-weight-bold">{{ $t('family.detail.visibility') }}</v-list-item-title>
            <v-list-item-subtitle>{{ $t(`family.management.visibility.${initialFamilyData.visibility.toLowerCase()}`) }}</v-list-item-subtitle>
          </v-list-item>
        </v-col>
      </v-row>
    </v-card-text>
    <v-card-text v-else>
      <v-alert type="info">{{ $t('family.detail.noFamilySelected') }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="$emit('close')">{{ $t('common.close') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import type { Family } from '@/types/family';

const props = defineProps<{
  initialFamilyData?: Family;
  readOnly?: boolean;
  title: string;
}>();
defineEmits(['close']);
</script>