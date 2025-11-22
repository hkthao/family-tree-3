<template>
  <v-container>
    <!-- Help Section -->
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 text-primary mb-4">{{ $t('help.title') }}</h1>
        <p class="text-body-1">
          {{ $t('help.description') }}
        </p>
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12">
        <v-card flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('help.gettingStarted.title') }}</v-card-title>
          <v-card-text>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-account-plus-outline">
                <v-list-item-title>{{ $t('help.gettingStarted.addMembers.title') }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ $t('help.gettingStarted.addMembers.subtitle') }}
                </v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-link-variant">
                <v-list-item-title>{{ $t('help.gettingStarted.editRelationships.title') }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ $t('help.gettingStarted.editRelationships.subtitle') }}
                </v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-magnify">
                <v-list-item-title>{{ $t('help.gettingStarted.searchTree.title') }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ $t('help.gettingStarted.searchTree.subtitle') }}
                </v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-robot-outline">
                <v-list-item-title>{{ $t('help.gettingStarted.useAIAssistant.title') }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ $t('help.gettingStarted.useAIAssistant.subtitle') }}
                </v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12">
        <FaqSection
          titleKey="help.faq.title"
          :faq-items="helpFaqItems"
        />
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12">
        <v-card flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('help.furtherAssistance.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">
              {{ $t('help.furtherAssistance.description') }}
              <router-link to="/terms" class="text-primary">{{ $t('terms.title') }}</router-link>,
              <router-link to="/privacy" class="text-primary">{{ $t('privacy.title') }}</router-link>, {{ $t('common.or') }}
              <a href="#" target="_blank" class="text-primary">{{ $t('help.furtherAssistance.documentationLink') }}</a> {{ $t('common.or') }}
              <a href="#" target="_blank" class="text-primary">{{ $t('help.furtherAssistance.contactSupport') }}</a>.
            </p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-divider class="my-8"></v-divider>

    <!-- Terms Section -->
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

    <v-divider class="my-8"></v-divider>

    <!-- Privacy Section -->
    <v-row>
      <v-col cols="12">
        <!-- Tiêu đề trang -->
        <h1 class="text-h4 text-primary mb-4">{{ t('privacy.title') }}</h1>
        <!-- Ngày hiệu lực -->
        <p class="text-body-2 text-medium-emphasis mb-6">{{ t('privacy.effectiveDate', { date: '2025-11-11' }) }}</p>

        <!-- Giới thiệu ngắn gọn -->
        <p class="text-body-1 mb-6">{{ t('privacy.introduction') }}</p>

        <!-- Các phần mở rộng cho từng mục -->
        <v-expansion-panels variant="accordion" class="mb-6">
          <!-- Dữ liệu chúng tôi thu thập -->
          <v-expansion-panel>
            <v-expansion-panel-title class="text-h6 text-secondary">{{ t('privacy.dataWeCollect.title') }}</v-expansion-panel-title>
            <v-expansion-panel-text>
              <p class="text-body-1">{{ t('privacy.dataWeCollect.paragraph1') }}</p>
              <v-list density="compact">
                <v-list-item prepend-icon="mdi-account-details-outline">
                  <v-list-item-title>{{ t('privacy.dataWeCollect.item1') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-graph-outline">
                  <v-list-item-title>{{ t('privacy.dataWeCollect.item2') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-robot-outline">
                  <v-list-item-title>{{ t('privacy.dataWeCollect.item3') }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-expansion-panel-text>
          </v-expansion-panel>

          <!-- Cách chúng tôi sử dụng dữ liệu -->
          <v-expansion-panel>
            <v-expansion-panel-title class="text-h6 text-secondary">{{ t('privacy.howWeUseData.title') }}</v-expansion-panel-title>
            <v-expansion-panel-text>
              <p class="text-body-1">{{ t('privacy.howWeUseData.paragraph1') }}</p>
              <v-list density="compact">
                <v-list-item prepend-icon="mdi-lightbulb-on-outline">
                  <v-list-item-title>{{ t('privacy.howWeUseData.item1') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-feature-search-outline">
                  <v-list-item-title>{{ t('privacy.howWeUseData.item2') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-database-check-outline">
                  <v-list-item-title>{{ t('privacy.howWeUseData.item3') }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-expansion-panel-text>
          </v-expansion-panel>

          <!-- Bảo vệ dữ liệu -->
          <v-expansion-panel>
            <v-expansion-panel-title class="text-h6 text-secondary">{{ t('privacy.dataProtection.title') }}</v-expansion-panel-title>
            <v-expansion-panel-text>
              <p class="text-body-1">{{ t('privacy.dataProtection.paragraph1') }}</p>
              <v-list density="compact">
                <v-list-item prepend-icon="mdi-lock-outline">
                  <v-list-item-title>{{ t('privacy.dataProtection.item1') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-account-key-outline">
                  <v-list-item-title>{{ t('privacy.dataProtection.item2') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-shield-check-outline">
                  <v-list-item-title>{{ t('privacy.dataProtection.item3') }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-expansion-panel-text>
          </v-expansion-panel>

          <!-- Dịch vụ của bên thứ ba -->
          <v-expansion-panel>
            <v-expansion-panel-title class="text-h6 text-secondary">{{ t('privacy.thirdPartyServices.title') }}</v-expansion-panel-title>
            <v-expansion-panel-text>
              <p class="text-body-1">{{ t('privacy.thirdPartyServices.paragraph1') }}</p>
              <v-list density="compact">
                <v-list-item prepend-icon="mdi-account-lock-outline">
                  <v-list-item-title>{{ t('privacy.thirdPartyServices.item1') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-bell-outline">
                  <v-list-item-title>{{ t('privacy.thirdPartyServices.item2') }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-expansion-panel-text>
          </v-expansion-panel>

          <!-- Quyền của người dùng -->
          <v-expansion-panel>
            <v-expansion-panel-title class="text-h6 text-secondary">{{ t('privacy.userRights.title') }}</v-expansion-panel-title>
            <v-expansion-panel-text>
              <p class="text-body-1">{{ t('privacy.userRights.paragraph1') }}</p>
              <v-list density="compact">
                <v-list-item prepend-icon="mdi-eye-outline">
                  <v-list-item-title>{{ t('privacy.userRights.item1') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-pencil-outline">
                  <v-list-item-title>{{ t('privacy.userRights.item2') }}</v-list-item-title>
                </v-list-item>
                <v-list-item prepend-icon="mdi-delete-outline">
                  <v-list-item-title>{{ t('privacy.userRights.item3') }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-expansion-panel-text>
          </v-expansion-panel>
        </v-expansion-panels>

        <!-- Phần liên hệ -->
        <v-card flat>
          <v-card-title class="text-h6 text-secondary">{{ t('privacy.contactSection.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">{{ t('privacy.contactSection.paragraph1') }}</p>
            <p class="text-body-1 mt-2">{{ t('privacy.contactSection.email') }}</p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { FaqSection } from '@/components/common'; // Import FaqSection

const { t } = useI18n();

const helpFaqItems = [
  { titleKey: 'help.faq.createTree.title', textKey: 'help.faq.createTree.text' },
  { titleKey: 'help.faq.importData.title', textKey: 'help.faq.importData.text' },
  { titleKey: 'help.faq.shareTree.title', textKey: 'help.faq.shareTree.text' },
];
</script>

<style scoped>
/* Không có style cụ thể cho trang này */
</style>