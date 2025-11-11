<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <!-- Tiêu đề trang -->
        <h1 class="text-h4 text-primary mb-4">{{ t('terms.title') }}</h1>
        <!-- Ngày cập nhật lần cuối -->
        <p class="text-body-2 text-medium-emphasis mb-6">{{ t('terms.lastUpdated', { date: '2025-11-11' }) }}</p>

        <!-- Giới thiệu -->
        <v-card flat class="mb-6">
          <v-card-title class="text-h6 text-secondary">{{ t('terms.introduction.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">{{ t('terms.introduction.paragraph1') }}</p>
            <p class="text-body-1 mt-2">{{ t('terms.introduction.paragraph2') }}</p>
          </v-card-text>
        </v-card>

        <v-divider class="my-4"></v-divider>

        <!-- Trách nhiệm của người dùng -->
        <v-card flat class="mb-6">
          <v-card-title class="text-h6 text-secondary">{{ t('terms.userResponsibilities.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">{{ t('terms.userResponsibilities.paragraph1') }}</p>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-check-circle-outline">
                <v-list-item-title>{{ t('terms.userResponsibilities.item1') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-check-circle-outline">
                <v-list-item-title>{{ t('terms.userResponsibilities.item2') }}</v-list-item-title>
              </v-list-item>
              <v-list-item prepend-icon="mdi-check-circle-outline">
                <v-list-item-title>{{ t('terms.userResponsibilities.item3') }}</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>

        <v-divider class="my-4"></v-divider>

        <!-- Quyền sở hữu dữ liệu -->
        <v-card flat class="mb-6">
          <v-card-title class="text-h6 text-secondary">{{ t('terms.dataOwnership.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">{{ t('terms.dataOwnership.paragraph1') }}</p>
            <p class="text-body-1 mt-2">{{ t('terms.dataOwnership.paragraph2') }}</p>
          </v-card-text>
        </v-card>

        <v-divider class="my-4"></v-divider>

        <!-- Tuyên bố từ chối trách nhiệm AI -->
        <v-card flat class="mb-6">
          <v-card-title class="text-h6 text-secondary">{{ t('terms.aiDisclaimer.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">{{ t('terms.aiDisclaimer.paragraph1') }}</p>
            <p class="text-body-1 mt-2">{{ t('terms.aiDisclaimer.paragraph2') }}</p>
          </v-card-text>
        </v-card>

        <v-divider class="my-4"></v-divider>

        <!-- Thay đổi điều khoản -->
        <v-card flat class="mb-6">
          <v-card-title class="text-h6 text-secondary">{{ t('terms.changesToTerms.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">{{ t('terms.changesToTerms.paragraph1') }}</p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>

<style scoped>
/* Không có style cụ thể cho trang này */
</style>
