<template>
    <v-row>
      <v-col cols="12">
        <PrivacySettings :family-id="familyId" />
      </v-col>
    </v-row>
</template>

<script setup lang="ts">
import { useFamilyDataManagement } from '@/composables';
import PrivacySettings from '@/components/family/PrivacySettings.vue'; // Import the new component

const props = defineProps<{
  familyId: string;
}>();

useFamilyDataManagement(props.familyId);
</script>

<style scoped>
/* Add any specific styles here */
</style>