<template>
  <v-card :elevation="0"  >
    <v-card-title class="text-h6 text-secondary">{{ $t(props.titleKey) }}</v-card-title>
    <v-card-text class="pa-0">
      <v-expansion-panels variant="accordion">
        <v-expansion-panel
          v-for="(item, index) in props.faqItems"
          :key="index"
          :title="$t(item.titleKey)"
        >
          <v-expansion-panel-text>
            {{ $t(item.textKey) }}
          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">

interface FaqItem {
  titleKey: string;
  textKey: string;
}

const props = defineProps<{
  titleKey: string;
  faqItems: FaqItem[];
}>();
</script>

<style scoped>
/* Add any specific styles for the FAQ section here */
</style>