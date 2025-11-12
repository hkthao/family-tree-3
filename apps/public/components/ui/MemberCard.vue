<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

interface Member {
  id: number;
  name: string;
  birthDate: string;
  deathDate?: string;
  imageUrl: string;
  shortBio: string;
}

defineProps<{
  member: Member;
}>();
</script>

<template>
  <div class="bg-white rounded-lg shadow-md overflow-hidden">
    <img
      :src="member.imageUrl"
      :alt="member.name"
      class="w-full h-48 object-cover"
    />
    <div class="p-4">
      <h3 class="text-xl font-semibold text-gray-800">{{ member.name }}</h3>
      <p class="text-sm text-gray-600">
        {{ member.birthDate }} - {{ member.deathDate || t('member.present') }}
      </p>
      <p class="text-gray-700 mt-2 line-clamp-3">{{ member.shortBio }}</p>
      <NuxtLink
        :to="`/member/${member.id}`"
        class="mt-4 inline-block text-indigo-600 hover:text-indigo-800 font-medium"
      >
        {{ t('viewDetails') }}
      </NuxtLink>
    </div>
  </div>
</template>
