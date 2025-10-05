<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('relationship.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded">
        <v-icon>{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="6">
              <MemberAutocomplete
                v-model="filters.sourceMemberId"
                :label="t('relationship.search.sourceMember')"
                clearable
              />
            </v-col>
            <v-col cols="12" md="6">
              <MemberAutocomplete
                v-model="filters.targetMemberId"
                :label="t('relationship.search.targetMember')"
                clearable
              />
            </v-col>
            <v-col cols="12" md="6">
              <v-select
                v-model="filters.type"
                :items="relationshipTypes"
                :label="t('relationship.search.type')"
                clearable
              ></v-select>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters">{{
            t('relationship.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters">{{ t('relationship.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { RelationshipFilter } from '@/types';
import { RelationshipType } from '@/types';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false);

const filters = ref<RelationshipFilter>({
  sourceMemberId: null,
  targetMemberId: null,
  type: null,
});

const relationshipTypes = computed(() => [
  { title: t('relationship.type.parent'), value: RelationshipType.Parent },
  { title: t('relationship.type.child'), value: RelationshipType.Child },
  { title: t('relationship.type.spouse'), value: RelationshipType.Spouse },
  { title: t('relationship.type.sibling'), value: RelationshipType.Sibling },
]);

watch(
  filters,
  () => {
    applyFilters();
  },
  { deep: true },
);

const applyFilters = () => {
  emit('update:filters', filters.value);
};

const resetFilters = () => {
  filters.value = {
    sourceMemberId: null,
    targetMemberId: null,
    type: null,
  };
  emit('update:filters', filters.value);
};

// Initial apply of filters on component mount
applyFilters();
</script>
