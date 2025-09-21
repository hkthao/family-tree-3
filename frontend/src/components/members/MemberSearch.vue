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
              <v-select
                v-model="filters.gender"
                :items="genderOptions"
                :label="t('member.search.gender')"
                clearable
              ></v-select>
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
              <v-autocomplete
                v-model="filters.familyId"
                :items="props.families"
                item-title="name"
                item-value="id"
                :label="t('member.search.family')"
                clearable
                :custom-filter="familyFilter"
              >
                <template #item="{ props, item }">
                  <v-list-item v-bind="props" :subtitle="item.raw.address"></v-list-item>
                </template>
              </v-autocomplete>
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
import type { Family } from '@/types/family';
import DateInputField from '@/components/common/DateInputField.vue';

const emit = defineEmits(['update:filters']);

const props = defineProps<{
  families: Family[];
}>();

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

const familyFilter = (_value: number, query: string, item: VuetifyInternalItem) => {
  if (!item || !item.raw) return false;

  const rawItem = item.raw;

  const name = rawItem.name ? String(rawItem.name).toLowerCase() : '';
  const address = rawItem.address ? String(rawItem.address).toLowerCase() : '';
  const searchText = query ? String(query).toLowerCase() : '';

  return name.includes(searchText) || address.includes(searchText);
};

interface VuetifyInternalItem {
  raw: Family;
}

const genderOptions = [
  { title: t('member.gender.male'), value: 'Male' },
  { title: t('member.gender.female'), value: 'Female' },
  { title: t('member.gender.other'), value: 'Other' },
];

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
