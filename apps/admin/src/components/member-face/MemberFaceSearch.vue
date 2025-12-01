<template>
  <v-expansion-panels flat class="mb-4">
    <v-expansion-panel>
      <v-expansion-panel-title class="pa-4 text-primary">
        <v-icon start>mdi-magnify</v-icon>
        <span class="font-weight-bold">{{ t('memberFace.search.title') }}</span>
      </v-expansion-panel-title>
      <v-expansion-panel-text>
        <v-row>
          <v-col cols="12" sm="6" md="4">
            <MemberAutocomplete
              v-model="internalFilters.memberId"
              :label="t('memberFace.filters.member')"
              clearable
              @update:model-value="applyFilters"
            />
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <FamilyAutocomplete
              v-model="internalFilters.familyId"
              :label="t('memberFace.filters.family')"
              clearable
              @update:model-value="applyFilters"
            />
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <v-select
              v-model="internalFilters.emotion"
              :label="t('memberFace.filters.emotion')"
              :items="emotionOptions"
              item-title="title"
              item-value="value"
              clearable
              @update:model-value="applyFilters"
            ></v-select>
          </v-col>
        </v-row>
      </v-expansion-panel-text>
    </v-expansion-panel>
  </v-expansion-panels>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import type { MemberFaceFilter } from '@/types';

interface MemberFaceSearchProps {
  initialFilters?: MemberFaceFilter;
}

const props = defineProps<MemberFaceSearchProps>();
const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const internalFilters = ref<MemberFaceFilter>(props.initialFilters || {});

const emotionOptions = computed(() => [
  { title: t('memberFace.emotion.happy'), value: 'happy' },
  { title: t('memberFace.emotion.sad'), value: 'sad' },
  { title: t('memberFace.emotion.angry'), value: 'angry' },
  { title: t('memberFace.emotion.surprise'), value: 'surprise' },
  { title: t('memberFace.emotion.fear'), value: 'fear' },
  { title: t('memberFace.emotion.disgust'), value: 'disgust' },
  { title: t('memberFace.emotion.neutral'), value: 'neutral' },
]);

watch(
  () => props.initialFilters,
  (newFilters) => {
    internalFilters.value = newFilters || {};
  },
  { deep: true }
);

const applyFilters = () => {
  emit('update:filters', internalFilters.value);
};
</script>