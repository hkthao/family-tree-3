<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('member.search.title') }}
      <v-spacer></v-spacer>
      <v-btn icon size="small" variant="text" @click="expanded = !expanded">
        <v-icon>{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="filters.fullName"
                :label="t('member.search.fullName')"
                clearable
                prepend-inner-icon="mdi-magnify"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <DateInputField
                v-model="filters.dateOfBirth"
                :label="t('member.search.dateOfBirth')"
                optional
              />
            </v-col>
            <v-col cols="12" md="4">
              <DateInputField
                v-model="filters.dateOfDeath"
                :label="t('member.search.dateOfDeath')"
                optional
              />
            </v-col>
            <v-col cols="12" md="4">
              <GenderSelect
                v-model="filters.gender"
                :label="t('member.search.gender')"
                clearable
              />
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="filters.placeOfBirth"
                :label="t('member.search.placeOfBirth')"
                clearable
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="filters.placeOfDeath"
                :label="t('member.search.placeOfDeath')"
                clearable
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="filters.occupation"
                :label="t('member.search.occupation')"
                clearable
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <FamilyAutocomplete
                v-model="filters.familyId"
                :label="t('member.search.family')"
                clearable
              />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters">{{ t('member.search.apply') }}</v-btn>
          <v-btn @click="resetFilters">{{ t('member.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberFilter } from '@/types/member';
import DateInputField from '@/components/common/DateInputField.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import GenderSelect from '@/components/common/GenderSelect.vue';

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed

const filters = ref<MemberFilter>({
  fullName: '',
  dateOfBirth: null,
  dateOfDeath: null,
  gender: undefined,
  placeOfBirth: '',
  placeOfDeath: '',
  occupation: '',
  familyId: undefined,
});

watch(filters.value, () => {
  // Debounce or apply immediately based on preference
  applyFilters();
}, { deep: true });

const applyFilters = () => {
  emit('update:filters', filters.value);
};

const resetFilters = () => {
  filters.value = {
    fullName: '',
    dateOfBirth: null,
    dateOfDeath: null,
    gender: undefined,
    placeOfBirth: '',
    placeOfDeath: '',
    occupation: '',
    familyId: undefined,
  };
  emit('update:filters', filters.value);
};
</script>
