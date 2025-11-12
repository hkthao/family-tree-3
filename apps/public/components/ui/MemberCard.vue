<template>
  <div class="bg-white rounded-lg shadow-lg overflow-hidden">
    <img :src="member.imageUrl" :alt="member.name" class="w-full h-48 object-cover" loading="lazy" />
    <div class="p-6">
      <h3 class="text-xl font-serif text-gray-800 mb-2">{{ member.name }}</h3>
      <p class="text-gray-600 text-sm">{{ member.bio }}</p>
      <NuxtLink :to="`/member/${member.id}`" class="mt-4 inline-block text-blue-500 hover:text-blue-600 font-medium">
        {{ $t('viewDetails') }}
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

interface Member {
  id: number;
  name: string;
  bio: string;
  imageUrl: string;
}

defineProps<{
  member: Member;
}>();
</script>
