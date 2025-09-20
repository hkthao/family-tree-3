<template>
  <v-card>
    <v-card-title class="d-flex align-center">
      <v-btn icon @click="$emit('back')" variant="text">
        <v-icon>mdi-arrow-left</v-icon>
      </v-btn>
      <span class="ml-2">{{ $t('family.detail.title') }}</span>
    </v-card-title>
    <v-card-text v-if="family">
      <v-row>
        <v-col cols="12" class="d-flex justify-center">
          <v-avatar size="100" v-if="family.avatarUrl">
            <v-img :src="family.avatarUrl" :alt="family.name"></v-img>
          </v-avatar>
          <v-avatar size="100" color="primary" v-else>
            <span class="text-h5">{{ family.name.charAt(0) }}</span>
          </v-avatar>
        </v-col>
        <v-col cols="12">
          <v-list-item>
            <v-list-item-title class="font-weight-bold">{{ $t('family.detail.name') }}</v-list-item-title>
            <v-list-item-subtitle>{{ family.name }}</v-list-item-subtitle>
          </v-list-item>
        </v-col>
        <v-col cols="12">
          <v-list-item>
            <v-list-item-title class="font-weight-bold">{{ $t('family.detail.description') }}</v-list-item-title>
            <v-list-item-subtitle>{{ family.description || $t('common.na') }}</v-list-item-subtitle>
          </v-list-item>
        </v-col>
        <v-col cols="12">
          <v-list-item>
            <v-list-item-title class="font-weight-bold">{{ $t('family.detail.visibility') }}</v-list-item-title>
            <v-list-item-subtitle>{{ $t(`family.management.visibility.${family.visibility.toLowerCase()}`) }}</v-list-item-subtitle>
          </v-list-item>
        </v-col>
      </v-row>
    </v-card-text>
    <v-card-text v-else>
      <v-alert type="info">{{ $t('family.detail.noFamilySelected') }}</v-alert>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import type { Props } from '@/types/family';

defineProps<Props>();
defineEmits(['back']);
</script>