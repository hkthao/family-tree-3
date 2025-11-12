<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const navigation = [
  { name: 'privacyPolicy', href: '/legal/privacy' },
  { name: 'termsOfUse', href: '/legal/terms' },
  { name: 'cookiePolicy', href: '/legal/cookies' },
];
</script>

<template>
  <footer class="bg-gray-800 text-gray-300 py-8">
    <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
      <div class="flex flex-col md:flex-row justify-between items-center">
        <div class="mb-4 md:mb-0 text-center md:text-left">
          <p class="text-lg font-semibold text-white mb-2">FamilyTree</p>
          <p class="text-sm">
            {{ t('familyTreeApp') }}
          </p>
        </div>
        <div class="flex flex-wrap justify-center md:justify-end space-x-4">
          <NuxtLink
            v-for="item in navigation"
            :key="item.name"
            :to="item.href"
            class="text-sm hover:text-white transition-colors duration-200"
          >
            {{ t(item.name) }}
          </NuxtLink>
        </div>
      </div>
      <div class="border-t border-gray-700 mt-8 pt-8 text-center text-sm">
        <p>&copy; {{ new Date().getFullYear() }} FamilyTree. {{ t('allRightsReserved') }}</p>
      </div>
    </div>
  </footer>
</template>
