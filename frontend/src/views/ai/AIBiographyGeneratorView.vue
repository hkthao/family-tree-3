<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 mb-2">{{ t('aiBiography.generator.title') }}</h1>
        <p class="text-subtitle-1 text-grey-darken-1">{{ t('aiBiography.generator.description') }}</p>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <AIBiographyInputPanel :member-id="memberId" />
      </v-col>
      <v-col cols="12" md="6">
        <AIBiographyResultPanel />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import AIBiographyInputPanel from '@/components/aiBiography/AIBiographyInputPanel.vue';
import AIBiographyResultPanel from '@/components/aiBiography/AIBiographyResultPanel.vue';

const { t } = useI18n();
const route = useRoute();

const memberId = ref<string>('');

onMounted(() => {
  if (route.params.memberId) {
    memberId.value = route.params.memberId as string;
  }
});
</script>
