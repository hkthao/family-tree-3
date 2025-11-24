<template>
  <StatisticCard
    :title="t('dashboard.averageAgeCard.title')"
    :value="averageAge !== undefined ? `${averageAge.toFixed(1)} ${t('dashboard.averageAgeCard.years')}` : t('dashboard.averageAgeCard.noData')"
    :icon="'mdi-account-clock-outline'"
    :color="'indigo'"
    :loading="loading"
  />
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import StatisticCard from './StatisticCard.vue';

const { t } = useI18n();

const { averageAge, loading } = defineProps<{
  averageAge: number | undefined;
  loading: boolean;
}>();
</script>