<template>
  <nav class="bg-white shadow-md fixed w-full z-10">
    <div class="container mx-auto px-4 py-4 flex justify-between items-center">
      <NuxtLink to="/" class="text-2xl font-serif text-gray-800">{{ $t('familyTree') }}</NuxtLink>
      <div class="hidden md:flex space-x-6">
        <NuxtLink to="/" class="text-gray-600 hover:text-gray-900">{{ $t('home') }}</NuxtLink>
        <NuxtLink to="/tree/1" class="text-gray-600 hover:text-gray-900">{{ $t('familyTree') }}</NuxtLink>
        <NuxtLink to="/stories" class="text-gray-600 hover:text-gray-900">{{ $t('stories') }}</NuxtLink>
        <NuxtLink to="/about" class="text-gray-600 hover:text-gray-900">{{ $t('about') }}</NuxtLink>
        <NuxtLink to="/contact" class="text-gray-600 hover:text-gray-900">{{ $t('contact') }}</NuxtLink>
      </div>
      <div class="flex items-center">
        <!-- Language Switcher -->
        <select v-model="locale" class="mr-4 p-2 rounded-md border border-gray-300">
          <option value="en">English</option>
          <option value="vi">Tiếng Việt</option>
        </select>

        <button @click="openSearchModal" class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-md">{{ $t('search') }}</button>
        <button @click="toggleMobileMenu" class="md:hidden ml-4 text-gray-600">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
          </svg>
        </button>
      </div>
    </div>

    <!-- Mobile Menu -->
    <div v-if="isMobileMenuOpen" class="md:hidden bg-white shadow-lg">
      <NuxtLink to="/" class="block px-4 py-2 text-gray-600 hover:bg-gray-100" @click="closeMobileMenu">{{ $t('home') }}</NuxtLink>
      <NuxtLink to="/tree/1" class="block px-4 py-2 text-gray-600 hover:bg-gray-100" @click="closeMobileMenu">{{ $t('familyTree') }}</NuxtLink>
      <NuxtLink to="/stories" class="block px-4 py-2 text-gray-600 hover:bg-gray-100" @click="closeMobileMenu">{{ $t('stories') }}</NuxtLink>
      <NuxtLink to="/about" class="block px-4 py-2 text-gray-600 hover:bg-gray-100" @click="closeMobileMenu">{{ $t('about') }}</NuxtLink>
      <NuxtLink to="/contact" class="block px-4 py-2 text-gray-600 hover:bg-gray-100" @click="closeMobileMenu">{{ $t('contact') }}</NuxtLink>
    </div>

    <!-- Search Modal -->
    <TransitionRoot as="template" :show="isSearchModalOpen">
      <Dialog as="div" class="relative z-10" @close="closeSearchModal">
        <TransitionChild as="template" enter="ease-out duration-300" enter-from="opacity-0" enter-to="opacity-100" leave="ease-in duration-200" leave-from="opacity-100" leave-to="opacity-0">
          <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" />
        </TransitionChild>

        <div class="fixed inset-0 z-10 w-screen overflow-y-auto">
          <div class="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <TransitionChild as="template" enter="ease-out duration-300" enter-from="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95" enter-to="opacity-100 translate-y-0 sm:scale-100" leave="ease-in duration-200" leave-from="opacity-100 translate-y-0 sm:scale-100" leave-to="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95">
              <DialogPanel class="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
                <div>
                  <div class="mt-3 text-center sm:mt-5">
                    <DialogTitle as="h3" class="text-base font-semibold leading-6 text-gray-900">{{ $t('searchMembers') }}</DialogTitle>
                    <div class="mt-2">
                      <SearchBox @search="handleSearch" />
                    </div>
                  </div>
                </div>
                <div class="mt-5 sm:mt-6">
                  <button type="button" class="inline-flex w-full justify-center rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600" @click="closeSearchModal">{{ $t('goBackToDashboard') }}</button>
                </div>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>
  </nav>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { Dialog, DialogPanel, DialogTitle, TransitionChild, TransitionRoot } from '@headlessui/vue';
import SearchBox from '@/components/ui/SearchBox.vue'; // Assuming SearchBox will be created here
import { useI18n } from 'vue-i18n';

const { locale, t } = useI18n();

const isMobileMenuOpen = ref(false);
const isSearchModalOpen = ref(false);

const toggleMobileMenu = () => {
  isMobileMenuOpen.value = !isMobileMenuOpen.value;
};

const closeMobileMenu = () => {
  isMobileMenuOpen.value = false;
};

const openSearchModal = () => {
  isSearchModalOpen.value = true;
};

const closeSearchModal = () => {
  isSearchModalOpen.value = false;
};

const handleSearch = (query: string) => {
  console.log('Search query:', query);
  // Implement actual search logic here, e.g., navigate to search page
  closeSearchModal();
  navigateTo(`/search?q=${query}`);
};
</script>

<style scoped>
/* Add any specific styles for Navbar here if needed */
</style>
