<template>
  <v-chip label size="small" class="text-capitalize">
    {{ genderTitle }}
  </v-chip>
</template>

<script setup lang="ts">
import { computed } from 'vue';

import { getGenderTitle } from '@/constants/genders'; // Assuming getGenderTitle exists here
import type { Gender } from '@/types'; // Assuming Gender enum is in '@/types'

interface Props {
  gender?: Gender;
}

const props = defineProps<Props>();


const genderTitle = computed(() => getGenderTitle(props.gender));
</script>