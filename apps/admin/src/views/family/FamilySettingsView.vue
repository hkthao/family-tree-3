<template>
    <v-row>
      <v-col cols="12">
        <PrivacySettings :family-id="props.familyId" />
      </v-col>
    </v-row>
</template>

<script setup lang="ts">
import PrivacySettings from '@/components/family/PrivacySettings.vue'; // Import the new component

const props = defineProps<{
  familyId: string;
}>();

</script>

<style scoped>
/* Add any specific styles here */
</style>