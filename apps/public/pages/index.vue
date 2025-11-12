<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member';
import MemberCard from '@/components/ui/MemberCard.vue';
import MemberCardSkeleton from '@/components/ui/MemberCardSkeleton.vue';

const { t } = useI18n();
const memberStore = useMemberStore();

// Fetch featured members on component mount
onMounted(() => {
  memberStore.fetchFeaturedMembers();
});

const featuredMembers = computed(() => memberStore.featuredMembers);
const loading = computed(() => memberStore.loading);
const error = computed(() => memberStore.error);
</script>

<template>
  <div>
    <!-- Hero Section -->
    <div
      class="relative bg-cover bg-center h-96 rounded-lg overflow-hidden"
      style="background-image: url('/images/hero-background.jpg')"
    >
      <div
        class="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4"
      >
        <div class="text-center text-white">
          <h1 class="text-4xl md:text-5xl font-bold mb-4">
            {{ t('exploreFamilyTree') }}
          </h1>
          <p class="text-lg md:text-xl mb-8">
            {{ t('preserveConnectShare') }}
          </p>
          <NuxtLink
            to="/tree"
            class="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-3 px-6 rounded-full transition duration-300"
          >
            {{ t('exploreButton') }}
          </NuxtLink>
        </div>
      </div>
    </div>

    <!-- Featured Stories Section -->
    <section class="py-16">
      <h2 class="text-3xl font-bold text-gray-800 text-center mb-10">
        {{ t('featuredStories') }}
      </h2>
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <MemberCardSkeleton v-for="n in 3" :key="n" />
      </div>
      <div v-else-if="error" class="text-center text-red-500">
        {{ error }}
      </div>
      <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <MemberCard
          v-for="member in featuredMembers"
          :key="member.id"
          :member="member"
        />
      </div>
    </section>

    <!-- Call-to-action Section -->
    <section class="bg-indigo-700 text-white py-16 rounded-lg">
      <div class="max-w-4xl mx-auto text-center p-4">
        <h2 class="text-3xl font-bold mb-4">
          {{ t('contributeToStory') }}
        </h2>
        <p class="text-lg mb-8">
          {{ t('shareYourStory') }}
        </p>
        <NuxtLink
          to="/contact"
          class="bg-white hover:bg-gray-100 text-indigo-700 font-bold py-3 px-6 rounded-full transition duration-300"
        >
          {{ t('contributeButton') }}
        </NuxtLink>
      </div>
    </section>
  </div>
</template>
