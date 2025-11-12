<script setup lang="ts">
import { Popover, PopoverButton, PopoverPanel } from '@headlessui/vue';
import { Bars3Icon, XMarkIcon } from '@heroicons/vue/24/outline';
import { GlobeAltIcon } from '@heroicons/vue/24/solid';
import { useI18n } from 'vue-i18n';

const { locale, t } = useI18n();

const navigation = [
  { name: 'home', href: '/' },
  { name: 'familyTree', href: '/tree' },
  { name: 'stories', href: '/stories' },
  { name: 'about', href: '/about' },
  { name: 'contact', href: '/contact' },
];

const changeLanguage = (lang: string) => {
  locale.value = lang;
};
</script>

<template>
  <Popover as="nav" class="relative bg-white shadow-md">
    <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <NuxtLink to="/" class="text-2xl font-bold text-indigo-600">
              FamilyTree
            </NuxtLink>
          </div>
          <div class="hidden md:block">
            <div class="ml-10 flex items-baseline space-x-4">
              <NuxtLink
                v-for="item in navigation"
                :key="item.name"
                :to="item.href"
                active-class="bg-indigo-500 text-white"
                class="text-gray-700 hover:bg-indigo-500 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
              >
                {{ t(item.name) }}
              </NuxtLink>
            </div>
          </div>
        </div>
        <div class="hidden md:block">
          <div class="ml-4 flex items-center md:ml-6">
            <Popover class="relative">
              <PopoverButton
                class="relative p-1 text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              >
                <span class="sr-only">{{ t('changeLanguage') }}</span>
                <GlobeAltIcon class="h-6 w-6" aria-hidden="true" />
              </PopoverButton>
              <transition
                enter-active-class="transition ease-out duration-100"
                enter-from-class="transform opacity-0 scale-95"
                enter-to-class="transform opacity-100 scale-100"
                leave-active-class="transition ease-in duration-75"
                leave-from-class="transform opacity-100 scale-100"
                leave-to-class="transform opacity-0 scale-95"
              >
                <PopoverPanel
                  class="absolute right-0 z-10 mt-2 w-32 origin-top-right rounded-md bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none"
                >
                  <a
                    href="#"
                    @click.prevent="changeLanguage('en')"
                    class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                    >English</a
                  >
                  <a
                    href="#"
                    @click.prevent="changeLanguage('vi')"
                    class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                    >Tiếng Việt</a
                  >
                </PopoverPanel>
              </transition>
            </Popover>
          </div>
        </div>
        <div class="-mr-2 flex md:hidden">
          <!-- Mobile menu button -->
          <PopoverButton
            class="inline-flex items-center justify-center p-2 rounded-md text-gray-400 hover:text-white hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-indigo-500 focus:ring-white"
          >
            <span class="sr-only">Open main menu</span>
            <Bars3Icon class="block h-6 w-6" aria-hidden="true" />
          </PopoverButton>
        </div>
      </div>
    </div>

    <PopoverPanel class="md:hidden">
      <div class="px-2 pt-2 pb-3 space-y-1 sm:px-3">
        <NuxtLink
          v-for="item in navigation"
          :key="item.name"
          :to="item.href"
          active-class="bg-indigo-500 text-white"
          class="text-gray-700 hover:bg-indigo-500 hover:text-white block px-3 py-2 rounded-md text-base font-medium"
        >
          {{ t(item.name) }}
        </NuxtLink>
      </div>
      <div class="pt-4 pb-3 border-t border-gray-700">
        <div class="flex items-center px-5">
          <div class="flex-shrink-0">
            <GlobeAltIcon class="h-6 w-6 text-gray-400" aria-hidden="true" />
          </div>
          <div class="ml-3">
            <div class="text-base font-medium leading-none text-gray-700">
              {{ t('language') }}
            </div>
            <div class="text-sm font-medium leading-none text-gray-500">
              {{ locale === 'en' ? 'English' : 'Tiếng Việt' }}
            </div>
          </div>
        </div>
        <div class="mt-3 px-2 space-y-1">
          <a
            href="#"
            @click.prevent="changeLanguage('en')"
            class="block px-3 py-2 rounded-md text-base font-medium text-gray-700 hover:bg-gray-100 hover:text-gray-900"
            >English</a
          >
          <a
            href="#"
            @click.prevent="changeLanguage('vi')"
            class="block px-3 py-2 rounded-md text-base font-medium text-gray-700 hover:bg-gray-100 hover:text-gray-900"
            >Tiếng Việt</a
          >
        </div>
      </div>
    </PopoverPanel>
  </Popover>
</template>
