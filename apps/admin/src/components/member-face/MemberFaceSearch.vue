<template>
  <v-card :elevation="0" class="mb-4" data-testid="member-face-search">
    <v-card-title class="text-h6 d-flex align-center  pa-0">
      <span>{{ t('common.advancedSearch') }}</span>
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded" data-testid="member-face-search-expand-button">
        <v-tooltip :text="expanded ? t('common.collapse') : t('common.expand')">
          <template v-slot:activator="{ props }">
            <v-icon v-bind="props">{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          </template>
        </v-tooltip>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text class="pa-0">
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
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters" data-testid="apply-filters-button">{{
            t('member.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters" data-testid="reset-filters-button">{{ t('member.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import type { MemberFaceFilter } from '@/types';

// const props = defineProps<MemberFaceSearchProps>(); // No props are being used anymore
const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed
const internalFilters = ref<MemberFaceFilter>({}); // Initialize directly to empty object

const emotionOptions = computed(() => [
  { title: t('memberFace.emotion.happy'), value: 'happy' },
  { title: t('memberFace.emotion.sad'), value: 'sad' },
  { title: t('memberFace.emotion.angry'), value: 'angry' },
  { title: t('memberFace.emotion.surprise'), value: 'surprise' },
  { title: t('memberFace.emotion.fear'), value: 'fear' },
  { title: t('memberFace.emotion.disgust'), value: 'disgust' },
  { title: t('memberFace.emotion.neutral'), value: 'neutral' },
]);

// onMounted hook removed as initialFilters prop is no longer used

watch(
  internalFilters.value,
  () => {
    // Apply filters immediately when internal filters change if not expanded,
    // otherwise wait for explicit apply button click.
    if (!expanded.value) {
      applyFilters(); // Revert to direct apply
    }
  },
  { deep: true }
);

const applyFilters = () => {
  emit('update:filters', internalFilters.value);
};

const resetFilters = () => {
  internalFilters.value = {}; // Reset all filters
  applyFilters(); // Revert to direct apply
};
</script>