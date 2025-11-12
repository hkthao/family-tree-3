<template>
  <BaseLayout>
    <!-- Hero Section -->
    <section class="relative bg-cover bg-center h-96 flex items-center justify-center text-white"
      style="background-image: url('/images/hero-background.jpg');">
      <div class="absolute inset-0 bg-black opacity-50"></div>
      <div class="relative z-10 text-center">
        <h1 class="text-5xl font-serif mb-4">{{ $t('exploreFamilyTree') }}</h1>
        <p class="text-xl mb-8">{{ $t('preserveConnectShare') }}</p>
        <NuxtLink to="/tree/1" class="bg-gold-500 hover:bg-gold-600 text-white font-bold py-3 px-8 rounded-full text-lg">
          {{ $t('exploreButton') }}
        </NuxtLink>
      </div>
    </section>

    <!-- Story Preview Section -->
    <section class="container mx-auto px-4 py-12">
      <h2 class="text-4xl font-serif text-center mb-8">{{ $t('featuredStories') }}</h2>
      <div v-if="memberStore.loading" class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <MemberCardSkeleton v-for="n in 3" :key="n" />
      </div>
      <div v-else-if="memberStore.error" class="text-center text-red-600 text-lg">
        {{ memberStore.error }}
      </div>
      <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <MemberCard v-for="member in featuredMembers" :key="member.id" :member="member" />
      </div>
    </section>

    <!-- Call-to-Action Section -->
    <section class="bg-cream-100 py-16 text-center">
      <h2 class="text-4xl font-serif mb-6">{{ $t('contributeToStory') }}</h2>
      <p class="text-lg mb-8">{{ $t('shareYourStory') }}</p>
      <NuxtLink to="/contact" class="bg-blue-500 hover:bg-blue-600 text-white font-bold py-3 px-8 rounded-full text-lg">
        {{ $t('contributeButton') }}
      </NuxtLink>
    </section>
  </BaseLayout>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue';
import BaseLayout from '@/components/layout/BaseLayout.vue';
import MemberCard from '@/components/ui/MemberCard.vue';
import MemberCardSkeleton from '@/components/ui/MemberCardSkeleton.vue';
import { useMemberStore } from '@/stores/member';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const memberStore = useMemberStore();

onMounted(() => {
  memberStore.fetchMembers();
});

// Use a computed property to get featured members from the store
const featuredMembers = computed(() => memberStore.members.slice(0, 3));
</script>

<style scoped>
/* Custom styles for gold color */
.bg-gold-500 {
  background-color: #FFD700; /* Gold color */
}
.hover\:bg-gold-600:hover {
  background-color: #E6C200; /* Darker gold on hover */
}
.bg-cream-100 {
  background-color: #FFFDD0; /* Cream color */
}
</style>
